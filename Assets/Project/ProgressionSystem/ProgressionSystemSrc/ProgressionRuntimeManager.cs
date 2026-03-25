using RainbowTower.Bootstrap;
using UnityEngine;

namespace RainbowTower.ProgressionSystem
{
    public sealed class ProgressionRuntimeManager : IRuntimeManager
    {
        private ProgressionPrototypeConfig progressionConfig;

        public int CurrentXp { get; private set; }

        public void AddXp(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            CurrentXp += amount;
        }

        public void Initialize(ServiceLocator serviceLocator)
        {
            progressionConfig = serviceLocator.ConfigurationProvider.GetConfiguration<ProgressionPrototypeConfig>();
            if (progressionConfig == null)
            {
                Debug.LogError("ProgressionRuntimeManager requires ProgressionPrototypeConfig.");
                CurrentXp = 0;
                return;
            }

            CurrentXp = progressionConfig.StartXp;
        }

        public void Tick(float deltaTime)
        {
        }

        public void LateTick(float deltaTime)
        {
        }

        public void Deinitialize()
        {
            progressionConfig = null;
            CurrentXp = 0;
        }
    }
}

