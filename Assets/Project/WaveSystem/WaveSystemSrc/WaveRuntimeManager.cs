using RainbowTower.Bootstrap;
using RainbowTower.EnemySystem;
using RainbowTower.MainUi;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RainbowTower.WaveSystem
{
    public sealed class WaveRuntimeManager : IRuntimeManager
    {
        private readonly EnemyRuntimeManager enemyRuntimeManager;

        private WavePrototypeConfig waveConfig;
        private MainUiProvider mainUiProvider;

        private int currentPlayerHp;
        private int maxPlayerHp;
        private int currentWave;
        private int enemiesToSpawnInWave;
        private int spawnedInWave;

        private float initialDelayTimer;
        private float betweenWavesTimer;
        private float spawnTimer;

        private bool isReady;
        private bool isSpawningWave;
        private bool isDefeated;

        public WaveRuntimeManager(EnemyRuntimeManager enemyRuntimeManager)
        {
            this.enemyRuntimeManager = enemyRuntimeManager;
        }

        public void Initialize(ServiceLocator serviceLocator)
        {
            waveConfig = serviceLocator.ConfigurationProvider.GetConfiguration<WavePrototypeConfig>();
            mainUiProvider = serviceLocator.MainUiProvider;

            if (waveConfig == null)
            {
                Debug.LogError("WaveRuntimeManager requires WavePrototypeConfig.");
                isReady = false;
                return;
            }

            if (mainUiProvider == null)
            {
                Debug.LogError("WaveRuntimeManager requires MainUiProvider.");
                isReady = false;
                return;
            }

            currentPlayerHp = Mathf.Max(1, waveConfig.StartPlayerHp);
            maxPlayerHp = currentPlayerHp;
            currentWave = 0;
            initialDelayTimer = Mathf.Max(0f, waveConfig.InitialDelay);
            betweenWavesTimer = 0f;
            spawnTimer = 0f;
            isSpawningWave = false;
            isDefeated = false;

            mainUiProvider.HideDefeatPopup();
            UpdateHud();
            isReady = true;
        }

        public void Tick(float deltaTime)
        {
            if (!isReady || isDefeated)
            {
                return;
            }

            if (initialDelayTimer > 0f)
            {
                initialDelayTimer -= deltaTime;
                if (initialDelayTimer <= 0f)
                {
                    StartNextWave();
                }

                return;
            }

            if (isSpawningWave)
            {
                ProcessWaveSpawning(deltaTime);
                return;
            }

            if (enemyRuntimeManager.ActiveEnemyCount > 0)
            {
                return;
            }

            if (currentWave >= waveConfig.TotalWaves)
            {
                return;
            }

            betweenWavesTimer -= deltaTime;
            if (betweenWavesTimer <= 0f)
            {
                StartNextWave();
            }
        }

        public void LateTick(float deltaTime)
        {
        }

        public void Deinitialize()
        {
            isReady = false;
            isSpawningWave = false;
            isDefeated = false;
            waveConfig = null;
            mainUiProvider = null;
        }

        private void ProcessWaveSpawning(float deltaTime)
        {
            if (spawnedInWave >= enemiesToSpawnInWave)
            {
                isSpawningWave = false;
                betweenWavesTimer = Mathf.Max(0.15f, waveConfig.TimeBetweenWaves);
                return;
            }

            spawnTimer -= deltaTime;
            if (spawnTimer > 0f)
            {
                return;
            }

            var enemy = enemyRuntimeManager.SpawnEnemy(OnEnemyReachedExit);
            if (enemy != null)
            {
                spawnedInWave++;
            }

            spawnTimer = Mathf.Max(0.1f, waveConfig.SpawnInterval);
        }

        private void StartNextWave()
        {
            currentWave++;
            enemiesToSpawnInWave = Mathf.Max(1, waveConfig.BaseEnemiesPerWave + (currentWave - 1) * waveConfig.EnemiesAddedPerWave);
            spawnedInWave = 0;
            spawnTimer = 0f;
            isSpawningWave = true;

            UpdateHud();
        }

        private void OnEnemyReachedExit(EnemyView escapedEnemy)
        {
            if (isDefeated)
            {
                return;
            }

            currentPlayerHp = Mathf.Max(0, currentPlayerHp - 1);
            UpdateHud();

            if (currentPlayerHp > 0)
            {
                return;
            }

            isDefeated = true;
            enemyRuntimeManager.DespawnAllEnemies();

            mainUiProvider.ShowDefeatPopup(() =>
            {
                var activeScene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(activeScene.buildIndex);
            });
        }

        private void UpdateHud()
        {
            mainUiProvider.SetHudValues(
                currentPlayerHp,
                maxPlayerHp,
                Mathf.Max(1, currentWave == 0 ? 1 : currentWave),
                Mathf.Max(1, waveConfig.TotalWaves));
        }
    }
}
