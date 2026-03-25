using RainbowTower.Bootstrap;
using RainbowTower.CombatFeedback;
using RainbowTower.CrystalSystem;
using RainbowTower.MainUi;
using UnityEngine;

namespace RainbowTower.ManaSystem
{
    public sealed class ManaRuntimeManager : IRuntimeManager
    {
        private readonly CrystalRuntimeManager crystalRuntimeManager;
        private readonly CombatFeedbackRuntimeManager combatFeedbackRuntimeManager;
        private readonly int[] manaValues = new int[ManaColorUtility.TotalColorCount];
        private readonly float[] generationCarry = new float[ManaColorUtility.TotalColorCount];

        private ManaPrototypeConfig manaConfig;
        private MainUiProvider mainUiProvider;
        private bool isReady;

        public ManaRuntimeManager(CrystalRuntimeManager crystalRuntimeManager, CombatFeedbackRuntimeManager combatFeedbackRuntimeManager)
        {
            this.crystalRuntimeManager = crystalRuntimeManager;
            this.combatFeedbackRuntimeManager = combatFeedbackRuntimeManager;
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

            var spent = TrySpendManaInternal(color, amount);
            if (spent)
            {
                UpdateCrystalPanels();
            }

            return spent;
        }

        public void AddMana(ManaColor color, int amount)
        {
            if (!isReady || amount <= 0)
            {
                return;
            }

            if (TryAddMana(color, amount))
            {
                UpdateCrystalPanels();
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

            for (var index = 0; index < ManaColorUtility.AllColors.Length; index++)
            {
                var color = ManaColorUtility.AllColors[index];
                manaValues[index] = 0;
                generationCarry[index] = 0f;
                TryAddMana(color, manaConfig.GetStartingMana(color));
            }

            isReady = true;
            UpdateCrystalPanels();
        }

        public void Tick(float deltaTime)
        {
            if (!isReady || !crystalRuntimeManager.IsReady)
            {
                return;
            }

            var anyManaChanged = false;

            for (var index = 0; index < ManaColorUtility.BaseColors.Length; index++)
            {
                var color = ManaColorUtility.BaseColors[index];
                var generationPerSecond = crystalRuntimeManager.GetGenerationPerSecond(color);
                if (generationPerSecond <= 0f)
                {
                    continue;
                }

                generationCarry[color.ToIndex()] += generationPerSecond * deltaTime;
                if (generationCarry[color.ToIndex()] < 1f)
                {
                    continue;
                }

                var generatedMana = Mathf.FloorToInt(generationCarry[color.ToIndex()]);
                generationCarry[color.ToIndex()] -= generatedMana;
                anyManaChanged |= TryAddMana(color, generatedMana);
            }

            for (var index = 0; index < ManaColorUtility.ConversionColors.Length; index++)
            {
                var conversionColor = ManaColorUtility.ConversionColors[index];
                if (!crystalRuntimeManager.IsUnlocked(conversionColor))
                {
                    continue;
                }

                if (!crystalRuntimeManager.TryGetGenerationInputs(conversionColor, out var inputColors) || inputColors == null || inputColors.Length == 0)
                {
                    continue;
                }

                var carryIndex = conversionColor.ToIndex();
                if (!HasEnoughInputMana(inputColors, 1))
                {
                    generationCarry[carryIndex] = Mathf.Min(generationCarry[carryIndex], 0.999f);
                    continue;
                }

                var generationPerSecond = crystalRuntimeManager.GetGenerationPerSecond(conversionColor);
                if (generationPerSecond <= 0f)
                {
                    continue;
                }

                generationCarry[carryIndex] += generationPerSecond * deltaTime;
                if (generationCarry[carryIndex] < 1f)
                {
                    continue;
                }

                var produceAttempts = Mathf.FloorToInt(generationCarry[carryIndex]);
                var producedCount = 0;

                for (var attempt = 0; attempt < produceAttempts; attempt++)
                {
                    if (IsAtCap(conversionColor) || !HasEnoughInputMana(inputColors, 1))
                    {
                        break;
                    }

                    for (var inputIndex = 0; inputIndex < inputColors.Length; inputIndex++)
                    {
                        TrySpendManaInternal(inputColors[inputIndex], 1);
                    }

                    if (!TryAddMana(conversionColor, 1))
                    {
                        break;
                    }

                    producedCount++;
                    anyManaChanged = true;
                }

                generationCarry[carryIndex] -= producedCount;
                if (producedCount < produceAttempts)
                {
                    generationCarry[carryIndex] = Mathf.Min(generationCarry[carryIndex], 0.999f);
                }
            }

            if (anyManaChanged)
            {
                UpdateCrystalPanels();
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

            for (var index = 0; index < ManaColorUtility.TotalColorCount; index++)
            {
                manaValues[index] = 0;
                generationCarry[index] = 0f;
            }
        }

        private bool HasEnoughInputMana(ManaColor[] inputColors, int requiredPerColor)
        {
            for (var index = 0; index < inputColors.Length; index++)
            {
                if (manaValues[inputColors[index].ToIndex()] < requiredPerColor)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsAtCap(ManaColor color)
        {
            var index = color.ToIndex();
            var cap = Mathf.Max(0, crystalRuntimeManager.GetManaCap(color));
            return manaValues[index] >= cap;
        }

        private bool TrySpendManaInternal(ManaColor color, int amount)
        {
            if (amount <= 0)
            {
                return false;
            }

            var index = color.ToIndex();
            if (manaValues[index] < amount)
            {
                return false;
            }

            manaValues[index] -= amount;
            return true;
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

        private void UpdateCrystalPanels()
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

            mainUiProvider.SetMixedCrystalPanelValues(
                manaValues[ManaColor.Yellow.ToIndex()],
                crystalRuntimeManager.GetCurrentLevel(ManaColor.Yellow),
                manaValues[ManaColor.Magenta.ToIndex()],
                crystalRuntimeManager.GetCurrentLevel(ManaColor.Magenta),
                manaValues[ManaColor.Cyan.ToIndex()],
                crystalRuntimeManager.GetCurrentLevel(ManaColor.Cyan));

            mainUiProvider.SetWhiteCrystalPanelValues(
                manaValues[ManaColor.White.ToIndex()],
                crystalRuntimeManager.GetCurrentLevel(ManaColor.White));
        }
    }
}




