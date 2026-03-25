using System;
using UnityEngine;

namespace RainbowTower.Bootstrap
{
    [CreateAssetMenu(
        fileName = "ConfigurationProvider",
        menuName = "RainbowTower/Bootstrap/Configuration Provider")]
    public sealed class ConfigurationProvider : ScriptableObject
    {
        [SerializeField] private ScriptableObject[] featureConfigurations = Array.Empty<ScriptableObject>();

        public TConfig GetConfiguration<TConfig>() where TConfig : ScriptableObject
        {
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