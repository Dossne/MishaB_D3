using RainbowTower.Bootstrap;
using RainbowTower.CrystalSystem;
using RainbowTower.EnemySystem;
using RainbowTower.ManaSystem;
using RainbowTower.ProgressionSystem;
using UnityEngine;

namespace RainbowTower.TowerSystem
{
    public sealed class TowerRuntimeManager : IRuntimeManager
    {
        private readonly EnemyRuntimeManager enemyRuntimeManager;
        private readonly ManaRuntimeManager manaRuntimeManager;
        private readonly CrystalRuntimeManager crystalRuntimeManager;
        private readonly ProgressionRuntimeManager progressionRuntimeManager;

        private TowerPrototypeConfig towerConfig;
        private float shotTimer;
        private bool isReady;

        public TowerRuntimeManager(
            EnemyRuntimeManager enemyRuntimeManager,
            ManaRuntimeManager manaRuntimeManager,
            CrystalRuntimeManager crystalRuntimeManager,
            ProgressionRuntimeManager progressionRuntimeManager)
        {
            this.enemyRuntimeManager = enemyRuntimeManager;
            this.manaRuntimeManager = manaRuntimeManager;
            this.crystalRuntimeManager = crystalRuntimeManager;
            this.progressionRuntimeManager = progressionRuntimeManager;
        }

        public void Initialize(ServiceLocator serviceLocator)
        {
            towerConfig = serviceLocator.ConfigurationProvider.GetConfiguration<TowerPrototypeConfig>();
            if (towerConfig == null)
            {
                Debug.LogError("TowerRuntimeManager requires TowerPrototypeConfig.");
                isReady = false;
                return;
            }

            shotTimer = GetCurrentShotInterval();
            isReady = true;
        }

        public void Tick(float deltaTime)
        {
            if (!isReady || !crystalRuntimeManager.IsReady)
            {
                return;
            }

            var currentShotInterval = GetCurrentShotInterval();
            if (shotTimer > currentShotInterval)
            {
                shotTimer = currentShotInterval;
            }

            shotTimer -= deltaTime;
            if (shotTimer > 0f)
            {
                return;
            }

            shotTimer = currentShotInterval;

            if (!enemyRuntimeManager.TryGetEnemyClosestToExit(out var targetEnemy))
            {
                return;
            }

            if (!crystalRuntimeManager.TryGetNextAttackColor(manaRuntimeManager, out var manaColor))
            {
                return;
            }

            if (!manaRuntimeManager.TrySpendMana(manaColor, 1))
            {
                return;
            }

            var damage = crystalRuntimeManager.GetShotDamage(manaColor);
            if (targetEnemy.ApplyDamage(damage) && targetEnemy.RewardXp > 0)
            {
                progressionRuntimeManager.AddXp(targetEnemy.RewardXp);
            }
        }

        public void LateTick(float deltaTime)
        {
        }

        public void Deinitialize()
        {
            isReady = false;
            shotTimer = 0f;
            towerConfig = null;
        }

        private float GetCurrentShotInterval()
        {
            var unlockedCrystalCount = Mathf.Max(1, crystalRuntimeManager.GetUnlockedBaseCrystalCount());
            var baseInterval = Mathf.Max(0.1f, towerConfig.ShotIntervalSeconds);
            return baseInterval / unlockedCrystalCount;
        }
    }
}