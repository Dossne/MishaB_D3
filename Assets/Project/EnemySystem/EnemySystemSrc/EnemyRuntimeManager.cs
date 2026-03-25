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
        private int spriteRotationIndex;
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
            spriteRotationIndex = 0;
            isReady = true;
        }

        public EnemyView SpawnEnemy(int hpBonus, int rewardXpBonus, Action<EnemyView> onReachedExit)
        {
            if (!isReady)
            {
                return null;
            }

            var enemyView = UnityEngine.Object.Instantiate(enemyConfig.EnemyPrefab, runtimeParent);
            var scaledRewardXp = Mathf.Max(0, enemyConfig.BaseRewardXp + Mathf.Max(0, rewardXpBonus));
            var sprite = ResolveEnemySprite();
            var tint = sprite != null ? Color.white : enemyConfig.EnemyTint;
            var adjustedScale = enemyConfig.EnemyScale * ResolveSpawnScaleMultiplier(sprite);

            enemyView.Initialize(
                cachedPathPoints,
                enemyConfig.MoveSpeed,
                tint,
                adjustedScale,
                Mathf.Max(1, enemyConfig.BaseHp + Mathf.Max(0, hpBonus)),
                scaledRewardXp,
                sprite,
                escapedEnemy =>
                {
                    activeEnemies.Remove(escapedEnemy);
                    onReachedExit?.Invoke(escapedEnemy);
                },
                killedEnemy =>
                {
                    activeEnemies.Remove(killedEnemy);
                });

            activeEnemies.Add(enemyView);
            return enemyView;
        }

        private Sprite ResolveEnemySprite()
        {
            var sprites = enemyConfig != null ? enemyConfig.EnemySprites : null;
            if (sprites == null || sprites.Length == 0)
            {
                return null;
            }

            for (var offset = 0; offset < sprites.Length; offset++)
            {
                var index = (spriteRotationIndex + offset) % sprites.Length;
                var candidate = sprites[index];
                if (candidate == null)
                {
                    continue;
                }

                spriteRotationIndex = (index + 1) % sprites.Length;
                return candidate;
            }

            return null;
        }

        private static float ResolveSpawnScaleMultiplier(Sprite sprite)
        {
            if (sprite == null || string.IsNullOrEmpty(sprite.name))
            {
                return 1f;
            }

            var tokens = sprite.name.Split('_');
            for (var index = 0; index < tokens.Length; index++)
            {
                if (!int.TryParse(tokens[index], out var spriteNumber))
                {
                    continue;
                }

                return spriteNumber switch
                {
                    1 => 0.75f,
                    2 => 0.875f,
                    3 => 1.125f,
                    4 => 1.35f,
                    _ => 1f
                };
            }

            return 1f;
        }

        public bool TryGetEnemyClosestToExit(out EnemyView enemy)
        {
            enemy = null;
            var bestProgress = float.MinValue;

            for (var index = 0; index < activeEnemies.Count; index++)
            {
                var candidate = activeEnemies[index];
                if (candidate == null || !candidate.IsAlive)
                {
                    continue;
                }

                if (candidate.ProgressToExit <= bestProgress)
                {
                    continue;
                }

                bestProgress = candidate.ProgressToExit;
                enemy = candidate;
            }

            return enemy != null;
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
