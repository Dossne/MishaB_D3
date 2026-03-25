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
            new BaseCrystalDefinition(ManaColor.Blue)
        };

        public BaseCrystalDefinition[] BaseCrystalDefinitions => baseCrystalDefinitions;

        public bool TryGetBaseDefinition(ManaColor color, out BaseCrystalDefinition definition)
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

        [Serializable]
        public sealed class BaseCrystalDefinition
        {
            [SerializeField] private ManaColor color;
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
            public int StartLevel => Mathf.Max(1, startLevel);
            public CrystalLevelData[] Levels => levels;
        }

        [Serializable]
        public sealed class CrystalLevelData
        {
            [SerializeField, Min(0.1f)] private float generationPerSecond = 1f;
            [SerializeField, Min(1)] private int manaCap = 10;
            [SerializeField, Min(1)] private int damage = 1;

            public float GenerationPerSecond => Mathf.Max(0.1f, generationPerSecond);
            public int ManaCap => Mathf.Max(1, manaCap);
            public int Damage => Mathf.Max(1, damage);
        }
    }
}

