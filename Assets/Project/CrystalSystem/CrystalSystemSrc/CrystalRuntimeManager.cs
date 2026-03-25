using RainbowTower.Bootstrap;
using RainbowTower.ManaSystem;
using UnityEngine;

namespace RainbowTower.CrystalSystem
{
    public sealed class CrystalRuntimeManager : IRuntimeManager
    {
        private static readonly ManaColor[] BaseRotation =
        {
            ManaColor.Red,
            ManaColor.Green,
            ManaColor.Blue
        };

        private static readonly CrystalPrototypeConfig.CrystalLevelData FallbackLevelData = new CrystalPrototypeConfig.CrystalLevelData();

        private readonly int[] levelsByColor = new int[ManaColorUtility.BaseColorCount];

        private CrystalPrototypeConfig crystalConfig;
        private int nextRotationIndex;

        public bool IsReady { get; private set; }

        public int GetCurrentLevel(ManaColor color)
        {
            return levelsByColor[color.ToIndex()];
        }

        public float GetGenerationPerSecond(ManaColor color)
        {
            return ResolveLevelData(color).GenerationPerSecond;
        }

        public int GetManaCap(ManaColor color)
        {
            return ResolveLevelData(color).ManaCap;
        }

        public int GetShotDamage(ManaColor color)
        {
            return ResolveLevelData(color).Damage;
        }

        public bool TryGetNextAttackColor(ManaRuntimeManager manaRuntimeManager, out ManaColor manaColor)
        {
            if (!IsReady)
            {
                manaColor = default;
                return false;
            }

            for (var offset = 0; offset < BaseRotation.Length; offset++)
            {
                var rotationIndex = (nextRotationIndex + offset) % BaseRotation.Length;
                var candidateColor = BaseRotation[rotationIndex];
                if (manaRuntimeManager.GetCurrentMana(candidateColor) <= 0)
                {
                    continue;
                }

                nextRotationIndex = (rotationIndex + 1) % BaseRotation.Length;
                manaColor = candidateColor;
                return true;
            }

            manaColor = default;
            return false;
        }

        public void Initialize(ServiceLocator serviceLocator)
        {
            crystalConfig = serviceLocator.ConfigurationProvider.GetConfiguration<CrystalPrototypeConfig>();
            if (crystalConfig == null)
            {
                Debug.LogError("CrystalRuntimeManager requires CrystalPrototypeConfig.");
                IsReady = false;
                return;
            }

            for (var index = 0; index < ManaColorUtility.BaseColorCount; index++)
            {
                var color = (ManaColor)index;
                if (!crystalConfig.TryGetBaseDefinition(color, out var definition) || definition == null)
                {
                    levelsByColor[index] = 1;
                    continue;
                }

                var maxLevel = Mathf.Max(1, definition.Levels == null ? 0 : definition.Levels.Length);
                levelsByColor[index] = Mathf.Clamp(definition.StartLevel, 1, maxLevel);
            }

            nextRotationIndex = 0;
            IsReady = true;
        }

        public void Tick(float deltaTime)
        {
        }

        public void LateTick(float deltaTime)
        {
        }

        public void Deinitialize()
        {
            for (var index = 0; index < levelsByColor.Length; index++)
            {
                levelsByColor[index] = 0;
            }

            crystalConfig = null;
            nextRotationIndex = 0;
            IsReady = false;
        }

        private CrystalPrototypeConfig.CrystalLevelData ResolveLevelData(ManaColor color)
        {
            if (crystalConfig == null || !crystalConfig.TryGetBaseDefinition(color, out var definition) || definition?.Levels == null || definition.Levels.Length == 0)
            {
                return FallbackLevelData;
            }

            var level = Mathf.Clamp(GetCurrentLevel(color), 1, definition.Levels.Length);
            var levelData = definition.Levels[level - 1];
            return levelData ?? FallbackLevelData;
        }
    }
}

