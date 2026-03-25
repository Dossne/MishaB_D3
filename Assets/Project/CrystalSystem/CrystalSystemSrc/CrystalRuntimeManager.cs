using System.Text;
using RainbowTower.Bootstrap;
using RainbowTower.MainUi;
using RainbowTower.ManaSystem;
using RainbowTower.ProgressionSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        private readonly ProgressionRuntimeManager progressionRuntimeManager;
        private readonly bool[] unlockedByColor = new bool[ManaColorUtility.BaseColorCount];
        private readonly int[] levelsByColor = new int[ManaColorUtility.BaseColorCount];
        private readonly CrystalUiEntry[] uiEntriesByColor = new CrystalUiEntry[ManaColorUtility.BaseColorCount];

        private CrystalPrototypeConfig crystalConfig;
        private MainUiProvider mainUiProvider;
        private TMP_FontAsset uiFont;
        private TMP_Text xpLabel;
        private int nextRotationIndex;
        private int lastPresentedXp;

        public CrystalRuntimeManager(ProgressionRuntimeManager progressionRuntimeManager)
        {
            this.progressionRuntimeManager = progressionRuntimeManager;
        }

        public bool IsReady { get; private set; }

        public bool IsUnlocked(ManaColor color)
        {
            return unlockedByColor[color.ToIndex()];
        }

        public int GetUnlockedBaseCrystalCount()
        {
            var unlockedCount = 0;
            for (var index = 0; index < unlockedByColor.Length; index++)
            {
                if (unlockedByColor[index])
                {
                    unlockedCount++;
                }
            }

            return Mathf.Max(1, unlockedCount);
        }
        public int GetCurrentLevel(ManaColor color)
        {
            return IsUnlocked(color) ? levelsByColor[color.ToIndex()] : 1;
        }

        public float GetGenerationPerSecond(ManaColor color)
        {
            return IsUnlocked(color) ? ResolveLevelData(color).GenerationPerSecond : 0f;
        }

        public int GetManaCap(ManaColor color)
        {
            return IsUnlocked(color) ? ResolveLevelData(color).ManaCap : 0;
        }

        public int GetShotDamage(ManaColor color)
        {
            return IsUnlocked(color) ? ResolveLevelData(color).Damage : 0;
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
                if (!IsUnlocked(candidateColor) || manaRuntimeManager.GetCurrentMana(candidateColor) <= 0)
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
            mainUiProvider = serviceLocator.MainUiProvider;
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
                    unlockedByColor[index] = true;
                    levelsByColor[index] = 1;
                    continue;
                }

                var maxLevel = Mathf.Max(1, definition.Levels == null ? 0 : definition.Levels.Length);
                unlockedByColor[index] = definition.StartUnlocked;
                levelsByColor[index] = definition.StartUnlocked
                    ? Mathf.Clamp(definition.StartLevel, 1, maxLevel)
                    : Mathf.Clamp(definition.StartLevel, 1, maxLevel);
            }

            nextRotationIndex = 0;
            lastPresentedXp = -1;
            SetupCrystalPanelUi();
            RefreshActionUi();
            IsReady = true;
        }

        public void Tick(float deltaTime)
        {
            if (!IsReady)
            {
                return;
            }

            if (!HasUiBindings())
            {
                SetupCrystalPanelUi();
                RefreshActionUi();
            }
            else if (progressionRuntimeManager.CurrentXp != lastPresentedXp)
            {
                RefreshActionUi();
            }
        }

        public void LateTick(float deltaTime)
        {
        }

        public void Deinitialize()
        {
            for (var index = 0; index < levelsByColor.Length; index++)
            {
                if (uiEntriesByColor[index]?.ActionButton != null)
                {
                    uiEntriesByColor[index].ActionButton.onClick.RemoveAllListeners();
                }

                unlockedByColor[index] = false;
                levelsByColor[index] = 0;
                uiEntriesByColor[index] = null;
            }

            crystalConfig = null;
            mainUiProvider = null;
            uiFont = null;
            xpLabel = null;
            nextRotationIndex = 0;
            lastPresentedXp = -1;
            IsReady = false;
        }

        private void OnRedAction()
        {
            TryPerformPrimaryAction(ManaColor.Red);
        }

        private void OnGreenAction()
        {
            TryPerformPrimaryAction(ManaColor.Green);
        }

        private void OnBlueAction()
        {
            TryPerformPrimaryAction(ManaColor.Blue);
        }

        private void TryPerformPrimaryAction(ManaColor color)
        {
            if (!IsReady)
            {
                return;
            }

            if (IsUnlocked(color))
            {
                TryUpgrade(color);
            }
            else
            {
                TryUnlock(color);
            }

            RefreshActionUi();
        }

        private bool TryUnlock(ManaColor color)
        {
            if (IsUnlocked(color) ||
                !crystalConfig.TryGetBaseDefinition(color, out var definition) ||
                definition == null ||
                !HasUnlockDependencies(definition))
            {
                return false;
            }

            var cost = definition.UnlockCost;
            if (cost > 0 && !progressionRuntimeManager.TrySpendXp(cost))
            {
                return false;
            }

            unlockedByColor[color.ToIndex()] = true;
            return true;
        }

        private bool TryUpgrade(ManaColor color)
        {
            if (!IsUnlocked(color) ||
                !crystalConfig.TryGetBaseDefinition(color, out var definition) ||
                definition?.Levels == null ||
                definition.Levels.Length == 0)
            {
                return false;
            }

            var currentLevel = GetCurrentLevel(color);
            if (currentLevel <= 0 || currentLevel >= definition.Levels.Length)
            {
                return false;
            }

            var levelData = definition.Levels[currentLevel - 1];
            var upgradeCost = levelData == null ? 0 : levelData.UpgradeCost;
            if (upgradeCost > 0 && !progressionRuntimeManager.TrySpendXp(upgradeCost))
            {
                return false;
            }

            levelsByColor[color.ToIndex()] = currentLevel + 1;
            return true;
        }

        private bool HasUnlockDependencies(CrystalPrototypeConfig.BaseCrystalDefinition definition)
        {
            var dependencies = definition.RequiredUnlockedColors;
            if (dependencies == null || dependencies.Length == 0)
            {
                return true;
            }

            for (var index = 0; index < dependencies.Length; index++)
            {
                if (!IsUnlocked(dependencies[index]))
                {
                    return false;
                }
            }

            return true;
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

        private void RefreshActionUi()
        {
            if (xpLabel != null)
            {
                xpLabel.text = $"XP {Mathf.Max(0, progressionRuntimeManager.CurrentXp)}";
            }

            UpdateActionUiForColor(ManaColor.Red);
            UpdateActionUiForColor(ManaColor.Green);
            UpdateActionUiForColor(ManaColor.Blue);
            lastPresentedXp = progressionRuntimeManager.CurrentXp;
        }

        private void UpdateActionUiForColor(ManaColor color)
        {
            var uiEntry = uiEntriesByColor[color.ToIndex()];
            if (uiEntry == null)
            {
                return;
            }

            if (!crystalConfig.TryGetBaseDefinition(color, out var definition) || definition == null)
            {
                SetUiEntryState(uiEntry, "Locked", "N/A", false);
                return;
            }

            var status = "Locked";
            var actionLabel = "Unlock";
            var interactable = false;

            if (!IsUnlocked(color))
            {
                if (!HasUnlockDependencies(definition))
                {
                    status = "Locked";
                    actionLabel = BuildDependencyLabel(definition.RequiredUnlockedColors);
                    interactable = false;
                }
                else
                {
                    status = "Locked";
                    actionLabel = $"Unlock {definition.UnlockCost} XP";
                    interactable = definition.UnlockCost <= 0 || progressionRuntimeManager.CanAfford(definition.UnlockCost);
                }
            }
            else
            {
                var currentLevel = GetCurrentLevel(color);
                if (definition.Levels != null && currentLevel > 0 && currentLevel < definition.Levels.Length)
                {
                    var levelData = definition.Levels[currentLevel - 1];
                    var upgradeCost = levelData == null ? 0 : levelData.UpgradeCost;
                    status = "Upgradeable";
                    actionLabel = $"Upgrade {upgradeCost} XP";
                    interactable = upgradeCost <= 0 || progressionRuntimeManager.CanAfford(upgradeCost);
                }
                else
                {
                    status = "Unlocked";
                    actionLabel = "Max Level";
                    interactable = false;
                }
            }

            SetUiEntryState(uiEntry, status, actionLabel, interactable);
        }

        private void SetUiEntryState(CrystalUiEntry uiEntry, string status, string actionLabel, bool interactable)
        {
            if (uiEntry.StatusLabel != null)
            {
                uiEntry.StatusLabel.text = status;
                uiEntry.StatusLabel.color = status switch
                {
                    "Upgradeable" => new Color(0.68f, 1f, 0.7f, 1f),
                    "Unlocked" => new Color(0.72f, 0.93f, 1f, 1f),
                    _ => new Color(1f, 0.82f, 0.52f, 1f)
                };
            }

            if (uiEntry.ActionLabel != null)
            {
                uiEntry.ActionLabel.text = actionLabel;
                uiEntry.ActionLabel.color = interactable ? new Color(0.24f, 0.15f, 0.08f, 1f) : new Color(0.47f, 0.47f, 0.47f, 1f);
            }

            if (uiEntry.ActionButton != null)
            {
                uiEntry.ActionButton.interactable = interactable;
            }

            if (uiEntry.ActionButtonImage != null)
            {
                uiEntry.ActionButtonImage.color = interactable
                    ? new Color(0.98f, 0.83f, 0.37f, 1f)
                    : new Color(0.58f, 0.58f, 0.58f, 0.92f);
            }
        }

        private void SetupCrystalPanelUi()
        {
            if (mainUiProvider == null || mainUiProvider.HudParent == null)
            {
                return;
            }

            var shelfPanel = mainUiProvider.HudParent.Find("CrystalShelfPanel") as RectTransform;
            if (shelfPanel == null)
            {
                return;
            }

            uiFont = ResolveUiFont(shelfPanel);
            xpLabel = EnsureXpLabel(shelfPanel);

            BindCrystalSlot(ManaColor.Red, "Red", OnRedAction);
            BindCrystalSlot(ManaColor.Green, "Green", OnGreenAction);
            BindCrystalSlot(ManaColor.Blue, "Blue", OnBlueAction);
        }

        private TMP_FontAsset ResolveUiFont(RectTransform shelfPanel)
        {
            var redLabelTransform = shelfPanel.Find("ShelfRows/TopRow/RedSlot/Label");
            if (redLabelTransform != null && redLabelTransform.TryGetComponent<TextMeshProUGUI>(out var redLabel) && redLabel.font != null)
            {
                return redLabel.font;
            }

            return null;
        }

        private TMP_Text EnsureXpLabel(RectTransform shelfPanel)
        {
            var existingLabel = shelfPanel.Find("XpLabel");
            if (existingLabel != null && existingLabel.TryGetComponent<TextMeshProUGUI>(out var existingText))
            {
                return existingText;
            }

            var xpObject = new GameObject("XpLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            var xpTransform = (RectTransform)xpObject.transform;
            xpTransform.SetParent(shelfPanel, false);
            xpTransform.anchorMin = new Vector2(1f, 1f);
            xpTransform.anchorMax = new Vector2(1f, 1f);
            xpTransform.offsetMin = new Vector2(-300f, -92f);
            xpTransform.offsetMax = new Vector2(-20f, -20f);
            xpTransform.localScale = Vector3.one;

            var text = xpObject.GetComponent<TextMeshProUGUI>();
            ConfigureText(text, "XP 0", 34f, FontStyles.Bold);
            text.alignment = TextAlignmentOptions.MidlineRight;
            text.color = new Color(1f, 0.89f, 0.5f, 1f);
            return text;
        }

        private void BindCrystalSlot(ManaColor color, string colorName, UnityEngine.Events.UnityAction onClick)
        {
            var shelfPanel = mainUiProvider.HudParent.Find("CrystalShelfPanel") as RectTransform;
            if (shelfPanel == null)
            {
                return;
            }

            var slot = shelfPanel.Find($"ShelfRows/TopRow/{colorName}Slot") as RectTransform;
            if (slot == null)
            {
                return;
            }

            var mainLabel = EnsureSlotLabel(slot, colorName);
            var statusLabel = EnsureStatusLabel(slot);
            var actionButton = EnsureActionButton(slot, out var actionImage);
            var actionLabel = EnsureActionButtonLabel(actionButton.transform as RectTransform);

            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(onClick);

            uiEntriesByColor[color.ToIndex()] = new CrystalUiEntry
            {
                MainLabel = mainLabel,
                StatusLabel = statusLabel,
                ActionButton = actionButton,
                ActionButtonImage = actionImage,
                ActionLabel = actionLabel
            };
        }

        private TMP_Text EnsureSlotLabel(RectTransform slot, string colorName)
        {
            var labelTransform = slot.Find("Label") as RectTransform;
            if (labelTransform == null)
            {
                var labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelTransform = (RectTransform)labelObject.transform;
                labelTransform.SetParent(slot, false);
            }

            labelTransform.anchorMin = Vector2.zero;
            labelTransform.anchorMax = Vector2.one;
            labelTransform.offsetMin = new Vector2(10f, 56f);
            labelTransform.offsetMax = new Vector2(-10f, -8f);
            labelTransform.localScale = Vector3.one;

            var label = labelTransform.GetComponent<TextMeshProUGUI>();
            ConfigureText(label, $"{colorName}\nM 0\nLv 1", 24f, FontStyles.Bold);
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.Normal;
            return label;
        }

        private TMP_Text EnsureStatusLabel(RectTransform slot)
        {
            var statusTransform = slot.Find("StatusLabel") as RectTransform;
            if (statusTransform == null)
            {
                var statusObject = new GameObject("StatusLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
                statusTransform = (RectTransform)statusObject.transform;
                statusTransform.SetParent(slot, false);
            }

            statusTransform.anchorMin = new Vector2(0f, 0f);
            statusTransform.anchorMax = new Vector2(1f, 0f);
            statusTransform.offsetMin = new Vector2(8f, 40f);
            statusTransform.offsetMax = new Vector2(-8f, 72f);
            statusTransform.localScale = Vector3.one;

            var status = statusTransform.GetComponent<TextMeshProUGUI>();
            ConfigureText(status, "Locked", 20f, FontStyles.Bold);
            status.alignment = TextAlignmentOptions.Center;
            return status;
        }

        private Button EnsureActionButton(RectTransform slot, out Image buttonImage)
        {
            var buttonTransform = slot.Find("ActionButton") as RectTransform;
            if (buttonTransform == null)
            {
                var buttonObject = new GameObject("ActionButton", typeof(RectTransform), typeof(Image), typeof(Button));
                buttonTransform = (RectTransform)buttonObject.transform;
                buttonTransform.SetParent(slot, false);
            }

            buttonTransform.anchorMin = new Vector2(0f, 0f);
            buttonTransform.anchorMax = new Vector2(1f, 0f);
            buttonTransform.offsetMin = new Vector2(8f, 6f);
            buttonTransform.offsetMax = new Vector2(-8f, 36f);
            buttonTransform.localScale = Vector3.one;

            buttonImage = buttonTransform.GetComponent<Image>();
            buttonImage.color = new Color(0.98f, 0.83f, 0.37f, 1f);

            var button = buttonTransform.GetComponent<Button>();
            button.targetGraphic = buttonImage;
            return button;
        }

        private TMP_Text EnsureActionButtonLabel(RectTransform buttonTransform)
        {
            var labelTransform = buttonTransform.Find("Label") as RectTransform;
            if (labelTransform == null)
            {
                var labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelTransform = (RectTransform)labelObject.transform;
                labelTransform.SetParent(buttonTransform, false);
            }

            labelTransform.anchorMin = Vector2.zero;
            labelTransform.anchorMax = Vector2.one;
            labelTransform.offsetMin = new Vector2(2f, 0f);
            labelTransform.offsetMax = new Vector2(-2f, 0f);
            labelTransform.localScale = Vector3.one;

            var label = labelTransform.GetComponent<TextMeshProUGUI>();
            ConfigureText(label, "Unlock", 20f, FontStyles.Bold);
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(0.24f, 0.15f, 0.08f, 1f);
            return label;
        }

        private void ConfigureText(TextMeshProUGUI text, string value, float size, FontStyles style)
        {
            if (uiFont != null)
            {
                text.font = uiFont;
            }
            else if (text.font == null && TMP_Settings.defaultFontAsset != null)
            {
                text.font = TMP_Settings.defaultFontAsset;
            }

            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.margin = Vector4.zero;
        }

        private bool HasUiBindings()
        {
            for (var index = 0; index < uiEntriesByColor.Length; index++)
            {
                if (uiEntriesByColor[index] == null)
                {
                    return false;
                }
            }

            return true;
        }

        private static string BuildDependencyLabel(ManaColor[] dependencies)
        {
            if (dependencies == null || dependencies.Length == 0)
            {
                return "Locked";
            }

            var builder = new StringBuilder("Need ");
            for (var index = 0; index < dependencies.Length; index++)
            {
                if (index > 0)
                {
                    builder.Append('+');
                }

                builder.Append(dependencies[index]);
            }

            return builder.ToString();
        }

        private sealed class CrystalUiEntry
        {
            public TMP_Text MainLabel;
            public TMP_Text StatusLabel;
            public Button ActionButton;
            public Image ActionButtonImage;
            public TMP_Text ActionLabel;
        }
    }
}

