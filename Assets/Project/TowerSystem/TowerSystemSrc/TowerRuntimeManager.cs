using RainbowTower.Bootstrap;
using RainbowTower.CombatFeedback;
using RainbowTower.CrystalSystem;
using RainbowTower.EnemySystem;
using RainbowTower.GameplayField;
using RainbowTower.ManaSystem;
using RainbowTower.ProgressionSystem;
using RainbowTower.TapDrops;
using UnityEngine;

namespace RainbowTower.TowerSystem
{
    public sealed class TowerRuntimeManager : IRuntimeManager
    {
        private readonly EnemyRuntimeManager enemyRuntimeManager;
        private readonly ManaRuntimeManager manaRuntimeManager;
        private readonly CrystalRuntimeManager crystalRuntimeManager;
        private readonly ProgressionRuntimeManager progressionRuntimeManager;
        private readonly CombatFeedbackRuntimeManager combatFeedbackRuntimeManager;
        private readonly TapDropRuntimeManager tapDropRuntimeManager;

        private TowerPrototypeConfig towerConfig;
        private Transform towerAnchor;
        private float shotTimer;
        private bool isReady;

        public TowerRuntimeManager(
            EnemyRuntimeManager enemyRuntimeManager,
            ManaRuntimeManager manaRuntimeManager,
            CrystalRuntimeManager crystalRuntimeManager,
            ProgressionRuntimeManager progressionRuntimeManager,
            CombatFeedbackRuntimeManager combatFeedbackRuntimeManager,
            TapDropRuntimeManager tapDropRuntimeManager)
        {
            this.enemyRuntimeManager = enemyRuntimeManager;
            this.manaRuntimeManager = manaRuntimeManager;
            this.crystalRuntimeManager = crystalRuntimeManager;
            this.progressionRuntimeManager = progressionRuntimeManager;
            this.combatFeedbackRuntimeManager = combatFeedbackRuntimeManager;
            this.tapDropRuntimeManager = tapDropRuntimeManager;
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

            if (serviceLocator.TryGet<GameplayFieldProvider>(out var fieldProvider))
            {
                towerAnchor = fieldProvider.TowerAnchor;
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
                combatFeedbackRuntimeManager?.NotifyInsufficientMana();
                return;
            }

            var manaCost = 1;
            var damageMultiplier = 1;
            tapDropRuntimeManager?.ResolveShotModifierForNextShot(
                manaColor,
                manaRuntimeManager.GetCurrentMana(manaColor),
                out manaCost,
                out damageMultiplier);

            if (!manaRuntimeManager.TrySpendMana(manaColor, manaCost))
            {
                combatFeedbackRuntimeManager?.NotifyInsufficientMana();
                return;
            }

            var damage = crystalRuntimeManager.GetShotDamage(manaColor) * Mathf.Max(1, damageMultiplier);
            var towerPosition = towerAnchor != null ? towerAnchor.position : Vector3.zero;
            var targetPosition = targetEnemy.transform.position;

            combatFeedbackRuntimeManager?.NotifyTowerShot(manaColor, towerPosition, targetPosition);

            var killedEnemy = targetEnemy.ApplyDamage(damage);
            combatFeedbackRuntimeManager?.NotifyEnemyHit(targetPosition, manaColor, damage, killedEnemy);

            tapDropRuntimeManager?.TrySpawnDropFromShot(towerPosition);

            if (killedEnemy)
            {
                combatFeedbackRuntimeManager?.NotifyEnemyDeath(targetPosition, targetEnemy.RewardXp);

                if (targetEnemy.RewardXp > 0)
                {
                    progressionRuntimeManager.AddXp(targetEnemy.RewardXp);
                }
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
            towerAnchor = null;
        }

        private float GetCurrentShotInterval()
        {
            var unlockedCrystalCount = Mathf.Max(1, crystalRuntimeManager.GetUnlockedBaseCrystalCount());
            var baseInterval = Mathf.Max(0.1f, towerConfig.ShotIntervalSeconds);
            return baseInterval / unlockedCrystalCount;
        }
    }
}
