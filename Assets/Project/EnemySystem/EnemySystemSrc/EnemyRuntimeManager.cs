using System;
using System.Collections.Generic;
using RainbowTower.Bootstrap;
using RainbowTower.GameplayField;
using UnityEngine;

namespace RainbowTower.EnemySystem
{
    public sealed class EnemyRuntimeManager : IRuntimeManager
    {
        private readonly List<EnemyView> activeEnemies = new();

        private EnemyPrototypeConfig enemyConfig;
        private Vector3[] cachedPathPoints;
        private Transform runtimeParent;
        private bool isReady;

        public int ActiveEnemyCount => activeEnemies.Count;

        public void Initialize(ServiceLocator serviceLocator)
        {
            enemyConfig = serviceLocator.ConfigurationProvider.GetConfiguration<EnemyPrototypeConfig>();
            if (enemyConfig == null || enemyConfig.EnemyPrefab == null)
            {
                Debug.LogError("EnemyRuntimeManager requires EnemyPrototypeConfig with enemy prefab.");
                isReady = false;
                return;
            }

            var fieldProvider = serviceLocator.GameplayFieldProvider;
            var pathDefinition = fieldProvider.PathDefinition;
            if (pathDefinition == null || pathDefinition.WaypointCount < 2)
            {
                Debug.LogError("EnemyRuntimeManager requires a valid GameplayPathDefinition.");
                isReady = false;
                return;
            }

            cachedPathPoints = new Vector3[pathDefinition.WaypointCount];
            for (var index = 0; index < pathDefinition.WaypointCount; index++)
            {
                cachedPathPoints[index] = pathDefinition.GetWaypointPosition(index);
            }

            var runtimeRootObject = new GameObject("RuntimeEnemies");
            runtimeParent = runtimeRootObject.transform;
            runtimeParent.SetParent(fieldProvider.FieldVisualRoot, false);

            activeEnemies.Clear();
            isReady = true;
        }

        public EnemyView SpawnEnemy(Action<EnemyView> onReachedExit)
        {
            if (!isReady)
            {
                return null;
            }

            var enemyView = UnityEngine.Object.Instantiate(enemyConfig.EnemyPrefab, runtimeParent);
            enemyView.Initialize(
                cachedPathPoints,
                enemyConfig.MoveSpeed,
                enemyConfig.EnemyTint,
                enemyConfig.EnemyScale,
                escapedEnemy =>
                {
                    activeEnemies.Remove(escapedEnemy);
                    onReachedExit?.Invoke(escapedEnemy);
                });

            activeEnemies.Add(enemyView);
            return enemyView;
        }

        public void DespawnAllEnemies()
        {
            for (var index = 0; index < activeEnemies.Count; index++)
            {
                if (activeEnemies[index] != null)
                {
                    UnityEngine.Object.Destroy(activeEnemies[index].gameObject);
                }
            }

            activeEnemies.Clear();
        }

        public void Tick(float deltaTime)
        {
            for (var index = activeEnemies.Count - 1; index >= 0; index--)
            {
                if (activeEnemies[index] == null)
                {
                    activeEnemies.RemoveAt(index);
                }
            }
        }

        public void LateTick(float deltaTime)
        {
        }

        public void Deinitialize()
        {
            DespawnAllEnemies();

            if (runtimeParent != null)
            {
                UnityEngine.Object.Destroy(runtimeParent.gameObject);
                runtimeParent = null;
            }

            cachedPathPoints = null;
            enemyConfig = null;
            isReady = false;
        }
    }
}
