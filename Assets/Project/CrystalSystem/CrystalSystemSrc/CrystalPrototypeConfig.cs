using System;
using RainbowTower.ManaSystem;
using UnityEngine;

namespace RainbowTower.CrystalSystem
{
    [CreateAssetMenu(
        fileName = "CrystalPrototypeConfig",
        menuName = "RainbowTower/CrystalSystem/Crystal Prototype Config")]
    public sealed class CrystalPrototypeConfig : ScriptableObject
    {
        [SerializeField] private BaseCrystalDefinition[] baseCrystalDefinitions =
        {
            new BaseCrystalDefinition(ManaColor.Red),
            new BaseCrystalDefinition(ManaColor.Green),
            new BaseCrystalDefinition(ManaColor.Blue),
            new BaseCrystalDefinition(ManaColor.Yellow),
            new BaseCrystalDefinition(ManaColor.Magenta),
            new BaseCrystalDefinition(ManaColor.Cyan)
        };

        public BaseCrystalDefinition[] BaseCrystalDefinitions => baseCrystalDefinitions;

        public bool TryGetDefinition(ManaColor color, out BaseCrystalDefinition definition)
        {
            for (var index = 0; index < baseCrystalDefinitions.Length; index++)
            {
                var candidate = baseCrystalDefinitions[index];
                if (candidate != null && candidate.Color == color)
                {
                    definition = candidate;
                    return true;
                }
            }

            definition = null;
            return false;
        }

        public bool TryGetBaseDefinition(ManaColor color, out BaseCrystalDefinition definition)
        {
            return TryGetDefinition(color, out definition);
        }

        [Serializable]
        public sealed class BaseCrystalDefinition
        {
            [SerializeField] private ManaColor color;
            [SerializeField] private bool startUnlocked = true;
            [SerializeField, Min(0)] private int unlockCost = 5;
            [SerializeField] private ManaColor[] requiredUnlockedColors = Array.Empty<ManaColor>();
            [SerializeField] private ManaColor[] generationInputColors = Array.Empty<ManaColor>();
            [SerializeField, Min(1)] private int startLevel = 1;
            [SerializeField] private CrystalLevelData[] levels =
            {
                new CrystalLevelData()
            };

            public BaseCrystalDefinition(ManaColor color)
            {
                this.color = color;
            }

            public ManaColor Color => color;
            public bool StartUnlocked => startUnlocked;
            public int UnlockCost => Mathf.Max(0, unlockCost);
            public ManaColor[] RequiredUnlockedColors => requiredUnlockedColors;
            public ManaColor[] GenerationInputColors => generationInputColors;
            public int StartLevel => Mathf.Max(1, startLevel);
            public CrystalLevelData[] Levels => levels;
        }

        [Serializable]
        public sealed class CrystalLevelData
        {
            [SerializeField, Min(0.1f)] private float generationPerSecond = 1f;
            [SerializeField, Min(1)] private int manaCap = 10;
            [SerializeField, Min(1)] private int damage = 1;
            [SerializeField, Min(0)] private int upgradeCost = 5;

            public float GenerationPerSecond => Mathf.Max(0.1f, generationPerSecond);
            public int ManaCap => Mathf.Max(1, manaCap);
            public int Damage => Mathf.Max(1, damage);
            public int UpgradeCost => Mathf.Max(0, upgradeCost);
        }
    }
}
