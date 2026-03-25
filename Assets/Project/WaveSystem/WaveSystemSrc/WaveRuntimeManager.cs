using RainbowTower.Bootstrap;
using RainbowTower.EnemySystem;
using RainbowTower.MainUi;
using RainbowTower.ProgressionSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RainbowTower.WaveSystem
{
    public sealed class WaveRuntimeManager : IRuntimeManager
    {
        private readonly EnemyRuntimeManager enemyRuntimeManager;
        private readonly ProgressionRuntimeManager progressionRuntimeManager;

        private WavePrototypeConfig waveConfig;
        private MainUiProvider mainUiProvider;

        private int currentPlayerHp;
        private int maxPlayerHp;
        private int currentWave;
        private int enemiesToSpawnInWave;
        private int spawnedInWave;
        private int currentWaveHpBonus;
        private int currentWaveRewardXpBonus;

        private float initialDelayTimer;
        private float betweenWavesTimer;
        private float spawnTimer;

        private bool isReady;
        private bool isSpawningWave;
        private bool isSessionEnded;
        private bool isWaveResolved;

        public WaveRuntimeManager(EnemyRuntimeManager enemyRuntimeManager, ProgressionRuntimeManager progressionRuntimeManager)
        {
            this.enemyRuntimeManager = enemyRuntimeManager;
            this.progressionRuntimeManager = progressionRuntimeManager;
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

            currentPlayerHp = waveConfig.StartPlayerHp;
            maxPlayerHp = currentPlayerHp;
            currentWave = 0;
            initialDelayTimer = waveConfig.InitialDelay;
            betweenWavesTimer = 0f;
            spawnTimer = 0f;
            currentWaveHpBonus = 0;
            currentWaveRewardXpBonus = 0;
            isSpawningWave = false;
            isSessionEnded = false;
            isWaveResolved = false;

            mainUiProvider.HideDefeatPopup();
            UpdateHud();
            isReady = true;
        }

        public void Tick(float deltaTime)
        {
            if (!isReady || isSessionEnded)
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

            if (currentWave <= 0)
            {
                return;
            }

            ResolveCurrentWaveIfNeeded();
            if (isSessionEnded)
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
            isSessionEnded = false;
            isWaveResolved = false;
            waveConfig = null;
            mainUiProvider = null;
        }

        private void ProcessWaveSpawning(float deltaTime)
        {
            if (spawnedInWave >= enemiesToSpawnInWave)
            {
                isSpawningWave = false;
                return;
            }

            spawnTimer -= deltaTime;
            if (spawnTimer > 0f)
            {
                return;
            }

            var enemy = enemyRuntimeManager.SpawnEnemy(currentWaveHpBonus, currentWaveRewardXpBonus, OnEnemyReachedExit);
            if (enemy != null)
            {
                spawnedInWave++;
            }

            spawnTimer = waveConfig.GetSpawnIntervalForWave(currentWave);
        }

        private void StartNextWave()
        {
            currentWave++;
            enemiesToSpawnInWave = waveConfig.GetEnemiesToSpawnForWave(currentWave);
            spawnedInWave = 0;
            currentWaveHpBonus = waveConfig.GetEnemyHpBonusForWave(currentWave);
            currentWaveRewardXpBonus = waveConfig.GetEnemyRewardXpBonusForWave(currentWave);
            spawnTimer = 0f;
            isSpawningWave = true;
            isWaveResolved = false;

            UpdateHud();
        }

        private void ResolveCurrentWaveIfNeeded()
        {
            if (isWaveResolved)
            {
                return;
            }

            isWaveResolved = true;
            GrantWaveClearXp(currentWave);

            if (currentWave >= waveConfig.TotalWaves)
            {
                CompleteSessionSuccess();
                return;
            }

            betweenWavesTimer = waveConfig.GetTimeBetweenWavesAfter(currentWave);
        }

        private void GrantWaveClearXp(int waveNumber)
        {
            var reward = progressionRuntimeManager.GetWaveClearXpReward(waveNumber);
            if (reward > 0)
            {
                progressionRuntimeManager.AddXp(reward);
            }
        }

        private void OnEnemyReachedExit(EnemyView escapedEnemy)
        {
            if (isSessionEnded)
            {
                return;
            }

            currentPlayerHp = Mathf.Max(0, currentPlayerHp - 1);
            UpdateHud();

            if (currentPlayerHp > 0)
            {
                return;
            }

            CompleteSessionDefeat();
        }

        private void CompleteSessionDefeat()
        {
            isSessionEnded = true;
            enemyRuntimeManager.DespawnAllEnemies();
            mainUiProvider.ShowDefeatPopup(RestartCurrentScene);
        }

        private void CompleteSessionSuccess()
        {
            isSessionEnded = true;
            enemyRuntimeManager.DespawnAllEnemies();
            mainUiProvider.ShowVictoryPopup($"You survived {waveConfig.TotalWaves} waves.", RestartCurrentScene);
        }

        private void RestartCurrentScene()
        {
            var activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }

        private void UpdateHud()
        {
            var shownWave = currentWave <= 0 ? 1 : Mathf.Clamp(currentWave, 1, waveConfig.TotalWaves);
            mainUiProvider.SetHudValues(
                currentPlayerHp,
                maxPlayerHp,
                shownWave,
                waveConfig.TotalWaves);
        }
    }
}
