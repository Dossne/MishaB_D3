using System;
using RainbowTower.EnemySystem;
using RainbowTower.GameplayField;
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
        [SerializeField] private ScriptableObject[] featureConfigurations = Array.Empty<ScriptableObject>();

        public GameplayFieldLayoutConfig GameplayFieldLayoutConfig => gameplayFieldLayoutConfig;
        public EnemyPrototypeConfig EnemyPrototypeConfig => enemyPrototypeConfig;
        public WavePrototypeConfig WavePrototypeConfig => wavePrototypeConfig;

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
