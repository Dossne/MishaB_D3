using RainbowTower.Bootstrap;
using RainbowTower.CrystalSystem;
using RainbowTower.MainUi;
using UnityEngine;

namespace RainbowTower.ManaSystem
{
    public sealed class ManaRuntimeManager : IRuntimeManager
    {
        private readonly CrystalRuntimeManager crystalRuntimeManager;
        private readonly int[] manaValues = new int[ManaColorUtility.BaseColorCount];
        private readonly float[] generationCarry = new float[ManaColorUtility.BaseColorCount];

        private ManaPrototypeConfig manaConfig;
        private MainUiProvider mainUiProvider;
        private bool isReady;

        public ManaRuntimeManager(CrystalRuntimeManager crystalRuntimeManager)
        {
            this.crystalRuntimeManager = crystalRuntimeManager;
        }

        public int GetCurrentMana(ManaColor color)
        {
            return manaValues[color.ToIndex()];
        }

        public bool TrySpendMana(ManaColor color, int amount)
        {
            if (!isReady || amount <= 0)
            {
                return false;
            }

            var index = color.ToIndex();
            if (manaValues[index] < amount)
            {
                return false;
            }

            manaValues[index] -= amount;
            UpdateBaseCrystalPanel();
            return true;
        }

        public void AddMana(ManaColor color, int amount)
        {
            if (!isReady || amount <= 0)
            {
                return;
            }

            if (TryAddMana(color, amount))
            {
                UpdateBaseCrystalPanel();
            }
        }

        public void Initialize(ServiceLocator serviceLocator)
        {
            manaConfig = serviceLocator.ConfigurationProvider.GetConfiguration<ManaPrototypeConfig>();
            mainUiProvider = serviceLocator.MainUiProvider;

            if (manaConfig == null)
            {
                Debug.LogError("ManaRuntimeManager requires ManaPrototypeConfig.");
                isReady = false;
                return;
            }

            for (var index = 0; index < ManaColorUtility.BaseColorCount; index++)
            {
                var color = (ManaColor)index;
                manaValues[index] = 0;
                generationCarry[index] = 0f;
                TryAddMana(color, manaConfig.GetStartingMana(color));
            }

            UpdateBaseCrystalPanel();
            isReady = true;
        }

        public void Tick(float deltaTime)
        {
            if (!isReady || !crystalRuntimeManager.IsReady)
            {
                return;
            }

            var anyManaChanged = false;
            for (var index = 0; index < ManaColorUtility.BaseColorCount; index++)
            {
                var color = (ManaColor)index;
                var generationPerSecond = crystalRuntimeManager.GetGenerationPerSecond(color);
                if (generationPerSecond <= 0f)
                {
                    continue;
                }

                generationCarry[index] += generationPerSecond * deltaTime;
                if (generationCarry[index] < 1f)
                {
                    continue;
                }

                var generatedMana = Mathf.FloorToInt(generationCarry[index]);
                generationCarry[index] -= generatedMana;
                anyManaChanged |= TryAddMana(color, generatedMana);
            }

            if (anyManaChanged)
            {
                UpdateBaseCrystalPanel();
            }
        }

        public void LateTick(float deltaTime)
        {
        }

        public void Deinitialize()
        {
            isReady = false;
            manaConfig = null;
            mainUiProvider = null;

            for (var index = 0; index < ManaColorUtility.BaseColorCount; index++)
            {
                manaValues[index] = 0;
                generationCarry[index] = 0f;
            }
        }

        private bool TryAddMana(ManaColor color, int amount)
        {
            if (amount <= 0)
            {
                return false;
            }

            var index = color.ToIndex();
            var cap = crystalRuntimeManager.GetManaCap(color);
            var previous = manaValues[index];
            var nextValue = Mathf.Min(cap, previous + amount);
            manaValues[index] = nextValue;
            return nextValue != previous;
        }

        private void UpdateBaseCrystalPanel()
        {
            if (mainUiProvider == null || !crystalRuntimeManager.IsReady)
            {
                return;
            }

            mainUiProvider.SetBaseCrystalPanelValues(
                manaValues[ManaColor.Red.ToIndex()],
                crystalRuntimeManager.GetCurrentLevel(ManaColor.Red),
                manaValues[ManaColor.Green.ToIndex()],
                crystalRuntimeManager.GetCurrentLevel(ManaColor.Green),
                manaValues[ManaColor.Blue.ToIndex()],
                crystalRuntimeManager.GetCurrentLevel(ManaColor.Blue));
        }
    }
}

