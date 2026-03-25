using System;
using System.Collections.Generic;
using RainbowTower.CombatFeedback;
using RainbowTower.CrystalSystem;
using RainbowTower.EnemySystem;
using RainbowTower.GameplayField;
using RainbowTower.ManaSystem;
using RainbowTower.ProgressionSystem;
using RainbowTower.TowerSystem;
using RainbowTower.WaveSystem;
using UnityEngine;

namespace RainbowTower.Bootstrap
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-1000)]
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private ServiceLocator serviceLocator;

        private readonly List<IRuntimeManager> runtimeManagers = new();
        private readonly List<IRuntimeManager> initializedManagers = new();
        private bool isInitialized;

        public ServiceLocator ServiceLocator => serviceLocator;

        private void Awake()
        {
            if (serviceLocator == null)
            {
                Debug.LogError("GameManager requires a ServiceLocator reference.", this);
                enabled = false;
                return;
            }

            serviceLocator.Initialize(this);
            InitializeSceneProviders();
            CreateRuntimeManagers();
            InitializeRuntimeManagers();
        }

        private void Update()
        {
            if (!isInitialized)
            {
                return;
            }

            var deltaTime = Time.deltaTime;
            for (var index = 0; index < initializedManagers.Count; index++)
            {
                initializedManagers[index].Tick(deltaTime);
            }
        }

        private void LateUpdate()
        {
            if (!isInitialized)
            {
                return;
            }

            var deltaTime = Time.deltaTime;
            for (var index = 0; index < initializedManagers.Count; index++)
            {
                initializedManagers[index].LateTick(deltaTime);
            }
        }

        private void OnDestroy()
        {
            DeinitializeRuntimeManagers();
        }

        public void RegisterRuntimeManager(IRuntimeManager runtimeManager)
        {
            if (runtimeManager == null)
            {
                throw new ArgumentNullException(nameof(runtimeManager));
            }

            runtimeManagers.Add(runtimeManager);
        }

        private void InitializeSceneProviders()
        {
            if (serviceLocator.TryGet<GameplayFieldProvider>(out var gameplayFieldProvider))
            {
                gameplayFieldProvider.Initialize(serviceLocator);
            }
        }

        private void CreateRuntimeManagers()
        {
            runtimeManagers.Clear();

            var progressionRuntimeManager = new ProgressionRuntimeManager();
            var crystalRuntimeManager = new CrystalRuntimeManager(progressionRuntimeManager);
            var combatFeedbackRuntimeManager = new CombatFeedbackRuntimeManager();
            var manaRuntimeManager = new ManaRuntimeManager(crystalRuntimeManager, combatFeedbackRuntimeManager);
            var enemyRuntimeManager = new EnemyRuntimeManager();
            var waveRuntimeManager = new WaveRuntimeManager(enemyRuntimeManager);
            var towerRuntimeManager = new TowerRuntimeManager(
                enemyRuntimeManager,
                manaRuntimeManager,
                crystalRuntimeManager,
                progressionRuntimeManager,
                combatFeedbackRuntimeManager);

            RegisterRuntimeManager(progressionRuntimeManager);
            RegisterRuntimeManager(crystalRuntimeManager);
            RegisterRuntimeManager(manaRuntimeManager);
            RegisterRuntimeManager(enemyRuntimeManager);
            RegisterRuntimeManager(waveRuntimeManager);
            RegisterRuntimeManager(towerRuntimeManager);
            RegisterRuntimeManager(combatFeedbackRuntimeManager);
        }

        private void InitializeRuntimeManagers()
        {
            initializedManagers.Clear();

            for (var index = 0; index < runtimeManagers.Count; index++)
            {
                var runtimeManager = runtimeManagers[index];
                runtimeManager.Initialize(serviceLocator);
                initializedManagers.Add(runtimeManager);
            }

            isInitialized = true;
        }

        private void DeinitializeRuntimeManagers()
        {
            if (!isInitialized)
            {
                return;
            }

            for (var index = initializedManagers.Count - 1; index >= 0; index--)
            {
                initializedManagers[index].Deinitialize();
            }

            initializedManagers.Clear();
            runtimeManagers.Clear();
            isInitialized = false;
        }
    }

    public interface IRuntimeManager
    {
        void Initialize(ServiceLocator serviceLocator);
        void Tick(float deltaTime);
        void LateTick(float deltaTime);
        void Deinitialize();
    }
}

