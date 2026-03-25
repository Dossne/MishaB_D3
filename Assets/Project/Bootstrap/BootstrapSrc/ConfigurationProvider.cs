using System;
using RainbowTower.GameplayField;
using UnityEngine;

namespace RainbowTower.Bootstrap
{
    [CreateAssetMenu(
        fileName = "ConfigurationProvider",
        menuName = "RainbowTower/Bootstrap/Configuration Provider")]
    public sealed class ConfigurationProvider : ScriptableObject
    {
        [SerializeField] private GameplayFieldLayoutConfig gameplayFieldLayoutConfig;
        [SerializeField] private ScriptableObject[] featureConfigurations = Array.Empty<ScriptableObject>();

        public GameplayFieldLayoutConfig GameplayFieldLayoutConfig => gameplayFieldLayoutConfig;

        public TConfig GetConfiguration<TConfig>() where TConfig : ScriptableObject
        {
            if (gameplayFieldLayoutConfig is TConfig typedGameplayFieldLayoutConfig)
            {
                return typedGameplayFieldLayoutConfig;
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
