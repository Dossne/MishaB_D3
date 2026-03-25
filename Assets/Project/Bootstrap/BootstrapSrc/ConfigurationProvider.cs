using System;
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
    [CreateAssetMenu(
        fileName = "ConfigurationProvider",
        menuName = "RainbowTower/Bootstrap/Configuration Provider")]
    public sealed class ConfigurationProvider : ScriptableObject
    {
        [SerializeField] private GameplayFieldLayoutConfig gameplayFieldLayoutConfig;
        [SerializeField] private EnemyPrototypeConfig enemyPrototypeConfig;
        [SerializeField] private WavePrototypeConfig wavePrototypeConfig;
        [SerializeField] private ManaPrototypeConfig manaPrototypeConfig;
        [SerializeField] private CrystalPrototypeConfig crystalPrototypeConfig;
        [SerializeField] private TowerPrototypeConfig towerPrototypeConfig;
        [SerializeField] private ProgressionPrototypeConfig progressionPrototypeConfig;
        [SerializeField] private CombatFeedbackConfig combatFeedbackConfig;
        [SerializeField] private ScriptableObject[] featureConfigurations = Array.Empty<ScriptableObject>();

        public GameplayFieldLayoutConfig GameplayFieldLayoutConfig => gameplayFieldLayoutConfig;
        public EnemyPrototypeConfig EnemyPrototypeConfig => enemyPrototypeConfig;
        public WavePrototypeConfig WavePrototypeConfig => wavePrototypeConfig;
        public ManaPrototypeConfig ManaPrototypeConfig => manaPrototypeConfig;
        public CrystalPrototypeConfig CrystalPrototypeConfig => crystalPrototypeConfig;
        public TowerPrototypeConfig TowerPrototypeConfig => towerPrototypeConfig;
        public ProgressionPrototypeConfig ProgressionPrototypeConfig => progressionPrototypeConfig;
        public CombatFeedbackConfig CombatFeedbackConfig => combatFeedbackConfig;

        public TConfig GetConfiguration<TConfig>() where TConfig : ScriptableObject
        {
            if (gameplayFieldLayoutConfig is TConfig typedGameplayFieldLayoutConfig)
            {
                return typedGameplayFieldLayoutConfig;
            }

            if (enemyPrototypeConfig is TConfig typedEnemyPrototypeConfig)
            {
                return typedEnemyPrototypeConfig;
            }

            if (wavePrototypeConfig is TConfig typedWavePrototypeConfig)
            {
                return typedWavePrototypeConfig;
            }

            if (manaPrototypeConfig is TConfig typedManaPrototypeConfig)
            {
                return typedManaPrototypeConfig;
            }

            if (crystalPrototypeConfig is TConfig typedCrystalPrototypeConfig)
            {
                return typedCrystalPrototypeConfig;
            }

            if (towerPrototypeConfig is TConfig typedTowerPrototypeConfig)
            {
                return typedTowerPrototypeConfig;
            }

            if (progressionPrototypeConfig is TConfig typedProgressionPrototypeConfig)
            {
                return typedProgressionPrototypeConfig;
            }

            if (combatFeedbackConfig is TConfig typedCombatFeedbackConfig)
            {
                return typedCombatFeedbackConfig;
            }

            for (var index = 0; index < featureConfigurations.Length; index++)
            {
                if (featureConfigurations[index] is TConfig typedConfiguration)
                {
                    return typedConfiguration;
                }
            }

            return null;
        }
    }
}

