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
        private static readonly ManaColor[] AttackRotation =
        {
            ManaColor.Red,
            ManaColor.Green,
            ManaColor.Blue,
            ManaColor.Yellow,
            ManaColor.Magenta,
            ManaColor.Cyan,
            ManaColor.White
        };

        private static readonly CrystalPrototypeConfig.CrystalLevelData FallbackLevelData = new CrystalPrototypeConfig.CrystalLevelData();

        private readonly ProgressionRuntimeManager progressionRuntimeManager;
        private readonly bool[] unlockedByColor = new bool[ManaColorUtility.TotalColorCount];
        private readonly int[] levelsByColor = new int[ManaColorUtility.TotalColorCount];
        private readonly CrystalUiEntry[] uiEntriesByColor = new CrystalUiEntry[ManaColorUtility.TotalColorCount];
        private readonly int[] lastPresentedManaByColor = new int[ManaColorUtility.TotalColorCount];

        private CrystalPrototypeConfig crystalConfig;
        private MainUiProvider mainUiProvider;
        private ManaRuntimeManager manaRuntimeManager;
        private TMP_FontAsset uiFont;
        private TMP_Text xpLabel;
        private int nextRotationIndex;
        private int lastPresentedXp;

        private RectTransform activePopupOverlay;
        private float storedTimeScale = 1f;
        private bool isPopupOpen;

        public CrystalRuntimeManager(ProgressionRuntimeManager progressionRuntimeManager)
        {
            this.progressionRuntimeManager = progressionRuntimeManager;
        }

        public bool IsReady { get; private set; }

        public void SetManaRuntimeManager(ManaRuntimeManager manager)
        {
            manaRuntimeManager = manager;
        }

        public bool IsUnlocked(ManaColor color)
        {
            return unlockedByColor[color.ToIndex()];
        }

        public int FillUnlockedColors(ManaColor[] outputBuffer)
        {
            if (outputBuffer == null || outputBuffer.Length == 0)
            {
                return 0;
            }

            var writeIndex = 0;
            for (var index = 0; index < ManaColorUtility.AllColors.Length && writeIndex < outputBuffer.Length; index++)
            {
                var color = ManaColorUtility.AllColors[index];
                if (!IsUnlocked(color))
                {
                    continue;
                }

                outputBuffer[writeIndex] = color;
                writeIndex++;
            }

            return writeIndex;
        }

        public int GetUnlockedBaseCrystalCount()
        {
            var unlockedCount = 0;
            for (var index = 0; index < ManaColorUtility.BaseColors.Length; index++)
            {
                if (IsUnlocked(ManaColorUtility.BaseColors[index]))
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

        public bool TryGetGenerationInputs(ManaColor color, out ManaColor[] generationInputs)
        {
            generationInputs = null;
            if (crystalConfig == null || !crystalConfig.TryGetDefinition(color, out var definition) || definition == null)
            {
                return false;
            }

            var inputColors = definition.GenerationInputColors;
            if (inputColors == null || inputColors.Length == 0)
            {
                return false;
            }

            generationInputs = inputColors;
            return true;
        }

        public int GetShotDamage(ManaColor color)
        {
            if (!IsUnlocked(color))
            {
                return 0;
            }

            var ownDamage = ResolveCurrentLevelDamage(color);
            if (!crystalConfig.TryGetDefinition(color, out var definition) || definition == null)
            {
                return ownDamage;
            }

            var parentColors = definition.GenerationInputColors;
            if (parentColors == null || parentColors.Length == 0)
            {
                return ownDamage;
            }

            var damage = ownDamage;
            for (var index = 0; index < parentColors.Length; index++)
            {
                damage += ResolveCurrentLevelDamage(parentColors[index]);
            }

            return damage;
        }

        public bool TryGetNextAttackColor(ManaRuntimeManager manaRuntimeManager, out ManaColor manaColor)
        {
            if (!IsReady)
            {
                manaColor = default;
                return false;
            }

            for (var offset = 0; offset < AttackRotation.Length; offset++)
            {
                var rotationIndex = (nextRotationIndex + offset) % AttackRotation.Length;
                var candidateColor = AttackRotation[rotationIndex];
                if (!IsUnlocked(candidateColor) || manaRuntimeManager.GetCurrentMana(candidateColor) <= 0)
                {
                    continue;
                }

                nextRotationIndex = (rotationIndex + 1) % AttackRotation.Length;
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

            for (var index = 0; index < ManaColorUtility.AllColors.Length; index++)
            {
                var color = ManaColorUtility.AllColors[index];
                if (!crystalConfig.TryGetDefinition(color, out var definition) || definition == null)
                {
                    unlockedByColor[index] = true;
                    levelsByColor[index] = 1;
                    continue;
                }

                var maxLevel = Mathf.Max(1, definition.Levels == null ? 0 : definition.Levels.Length);
                unlockedByColor[index] = definition.StartUnlocked;
                levelsByColor[index] = Mathf.Clamp(definition.StartLevel, 1, maxLevel);
            }

            nextRotationIndex = 0;
            lastPresentedXp = -1;
            SetupCrystalPanelUi();
            BindCheatUi();
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
                BindCheatUi();
                RefreshActionUi();
            }
            else if (progressionRuntimeManager.CurrentXp != lastPresentedXp || HasAnyManaValueChanged())
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
                if (uiEntriesByColor[index]?.SlotButton != null)
                {
                    uiEntriesByColor[index].SlotButton.onClick.RemoveAllListeners();
                }

                unlockedByColor[index] = false;
                levelsByColor[index] = 0;
                uiEntriesByColor[index] = null;
                lastPresentedManaByColor[index] = 0;
            }

            CloseActivePopup();

            if (mainUiProvider != null)
            {
                mainUiProvider.BindUnlockAllCrystalsCheat(null);
            }

            crystalConfig = null;
            mainUiProvider = null;
            manaRuntimeManager = null;
            uiFont = null;
            xpLabel = null;
            nextRotationIndex = 0;
            lastPresentedXp = -1;
            IsReady = false;
        }
        private void OnUnlockAllCheatAction()
        {
            if (!IsReady)
            {
                return;
            }

            for (var index = 0; index < ManaColorUtility.AllColors.Length; index++)
            {
                unlockedByColor[index] = true;
            }

            RefreshActionUi();
        }

        private void OnSlotPressed(ManaColor color)
        {
            if (!IsReady || crystalConfig == null || !crystalConfig.TryGetDefinition(color, out var definition) || definition == null)
            {
                return;
            }

            if (!IsUnlocked(color))
            {
                if (!HasUnlockDependencies(definition) || !CanAffordOrFree(definition.UnlockCost))
                {
                    return;
                }

                OpenUnlockPopup(color, definition);
                return;
            }

            OpenUpgradePopup(color, definition);
        }

        private bool TryUnlock(ManaColor color)
        {
            if (IsUnlocked(color) ||
                !crystalConfig.TryGetDefinition(color, out var definition) ||
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
                !crystalConfig.TryGetDefinition(color, out var definition) ||
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

            var currentLevelData = definition.Levels[currentLevel - 1];
            var upgradeCost = currentLevelData == null ? 0 : currentLevelData.UpgradeCost;
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
            if (crystalConfig == null || !crystalConfig.TryGetDefinition(color, out var definition) || definition?.Levels == null || definition.Levels.Length == 0)
            {
                return FallbackLevelData;
            }

            var level = Mathf.Clamp(GetCurrentLevel(color), 1, definition.Levels.Length);
            var levelData = definition.Levels[level - 1];
            return levelData ?? FallbackLevelData;
        }

        private int ResolveCurrentLevelDamage(ManaColor color)
        {
            return ResolveLevelData(color).Damage;
        }

        private bool HasAnyManaValueChanged()
        {
            if (manaRuntimeManager == null)
            {
                return false;
            }

            for (var index = 0; index < ManaColorUtility.AllColors.Length; index++)
            {
                var color = ManaColorUtility.AllColors[index];
                var currentMana = Mathf.Max(0, manaRuntimeManager.GetCurrentMana(color));
                if (currentMana != lastPresentedManaByColor[color.ToIndex()])
                {
                    return true;
                }
            }

            return false;
        }

        private void RefreshActionUi()
        {
            if (xpLabel != null)
            {
                xpLabel.text = $"XP {Mathf.Max(0, progressionRuntimeManager.CurrentXp)}";
            }

            for (var index = 0; index < ManaColorUtility.AllColors.Length; index++)
            {
                var color = ManaColorUtility.AllColors[index];
                UpdateActionUiForColor(color);
                lastPresentedManaByColor[color.ToIndex()] = manaRuntimeManager != null ? Mathf.Max(0, manaRuntimeManager.GetCurrentMana(color)) : 0;
            }

            lastPresentedXp = progressionRuntimeManager.CurrentXp;
        }

        private void UpdateActionUiForColor(ManaColor color)
        {
            var uiEntry = uiEntriesByColor[color.ToIndex()];
            if (uiEntry == null)
            {
                return;
            }

            if (!crystalConfig.TryGetDefinition(color, out var definition) || definition == null)
            {
                SetSlotInteractableState(uiEntry, false, null);
                return;
            }

            if (uiEntry.MainLabel != null)
            {
                var currentMana = manaRuntimeManager != null ? manaRuntimeManager.GetCurrentMana(color) : 0;
                uiEntry.MainLabel.text = $"{Mathf.Max(0, currentMana)}";
            }
if (!IsUnlocked(color))
            {
                var canUnlock = HasUnlockDependencies(definition) && CanAffordOrFree(definition.UnlockCost);
                SetSlotInteractableState(uiEntry, canUnlock, canUnlock ? mainUiProvider?.LockIndicatorSprite : null);
                return;
            }

            var currentLevel = GetCurrentLevel(color);
            var hasNextLevel = definition.Levels != null && currentLevel > 0 && currentLevel < definition.Levels.Length;
            var canUpgrade = hasNextLevel && CanAffordOrFree(definition.Levels[currentLevel - 1]?.UpgradeCost ?? 0);
            SetSlotInteractableState(uiEntry, true, canUpgrade ? mainUiProvider?.UpgradeIndicatorSprite : null);
        }

        private void SetSlotInteractableState(CrystalUiEntry uiEntry, bool interactable, Sprite indicatorSprite)
        {
            if (uiEntry.SlotButton != null)
            {
                uiEntry.SlotButton.interactable = interactable;
            }

            if (uiEntry.IndicatorImage != null)
            {
                if (indicatorSprite == null)
                {
                    uiEntry.IndicatorImage.sprite = null;
                    uiEntry.IndicatorImage.gameObject.SetActive(false);
                }
                else
                {
                    uiEntry.IndicatorImage.sprite = indicatorSprite;
                    uiEntry.IndicatorImage.color = Color.white;
                    uiEntry.IndicatorImage.gameObject.SetActive(true);
                }
            }
        }

        private void BindCheatUi()
        {
            mainUiProvider?.BindUnlockAllCrystalsCheat(OnUnlockAllCheatAction);
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
            xpLabel = EnsureXpLabel(mainUiProvider.HudParent, shelfPanel);

            BindCrystalSlot(ManaColor.Red, "TopRow", "Red");
            BindCrystalSlot(ManaColor.Green, "TopRow", "Green");
            BindCrystalSlot(ManaColor.Blue, "TopRow", "Blue");

            BindCrystalSlot(ManaColor.Yellow, "MiddleRow", "Yellow");
            BindCrystalSlot(ManaColor.Magenta, "MiddleRow", "Magenta");
            BindCrystalSlot(ManaColor.Cyan, "MiddleRow", "Cyan");
            BindCrystalSlot(ManaColor.White, "BottomRow", "White");
        }
        private TMP_FontAsset ResolveUiFont(RectTransform shelfPanel)
        {
            if (mainUiProvider != null)
            {
                if (mainUiProvider.HpLabel != null && mainUiProvider.HpLabel.font != null)
                {
                    return mainUiProvider.HpLabel.font;
                }

                if (mainUiProvider.WaveLabel != null && mainUiProvider.WaveLabel.font != null)
                {
                    return mainUiProvider.WaveLabel.font;
                }
            }

            var redLabelTransform = shelfPanel.Find("ShelfRows/TopRow/RedSlot/ManaLabel");
            if (redLabelTransform != null && redLabelTransform.TryGetComponent<TextMeshProUGUI>(out var redLabel) && redLabel.font != null)
            {
                return redLabel.font;
            }

            var anyLabel = shelfPanel.GetComponentInChildren<TextMeshProUGUI>(true);
            if (anyLabel != null && anyLabel.font != null)
            {
                return anyLabel.font;
            }

            return TMP_Settings.defaultFontAsset;
        }

        private TMP_Text EnsureXpLabel(RectTransform hudParent, RectTransform shelfPanel)
        {
            var oldShelfLabel = shelfPanel != null ? shelfPanel.Find("XpLabel") : null;
            if (oldShelfLabel != null)
            {
                Object.Destroy(oldShelfLabel.gameObject);
            }

            var hpPanel = hudParent != null ? hudParent.Find("TopHudPanel/HpPanel") as RectTransform : null;
            var topPanel = hudParent != null ? hudParent.Find("TopHudPanel") as RectTransform : null;
            var parent = hpPanel != null ? hpPanel : topPanel;
            if (parent == null)
            {
                return null;
            }

            var existingLabel = parent.Find("XpLabel");
            if (existingLabel != null && existingLabel.TryGetComponent<TextMeshProUGUI>(out var existingText))
            {
                return existingText;
            }

            var xpObject = new GameObject("XpLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            var xpTransform = (RectTransform)xpObject.transform;
            xpTransform.SetParent(parent, false);
            xpTransform.anchorMin = new Vector2(0f, 0f);
            xpTransform.anchorMax = new Vector2(1f, 1f);
            xpTransform.offsetMin = new Vector2(12f, 8f);
            xpTransform.offsetMax = new Vector2(-12f, -8f);
            xpTransform.localScale = Vector3.one;

            var text = xpObject.GetComponent<TextMeshProUGUI>();
            ConfigureText(text, "XP 0", 34f, FontStyles.Bold);
            text.alignment = TextAlignmentOptions.MidlineRight;
            text.color = new Color(1f, 0.89f, 0.5f, 1f);
            return text;
        }

        private void BindCrystalSlot(ManaColor color, string rowName, string colorName)
        {
            var shelfPanel = mainUiProvider.HudParent.Find("CrystalShelfPanel") as RectTransform;
            if (shelfPanel == null)
            {
                return;
            }

            var slot = shelfPanel.Find($"ShelfRows/{rowName}/{colorName}Slot") as RectTransform;
            if (slot == null)
            {
                return;
            }

            var iconImage = EnsureSlotIcon(slot, color);
            var mainLabel = EnsureManaLabel(slot);
            DisableLevelLabel(slot);
            TMP_Text levelLabel = null;
            DisableStatusLabel(slot);
            HideOldActionButton(slot);
            var slotButton = EnsureSlotButton(slot, iconImage, out var slotButtonGraphic);
            if (slotButton == null)
            {
                return;
            }
            var stateIndicator = EnsureStateIndicator(slot, iconImage);

            slotButton.onClick.RemoveAllListeners();
            slotButton.onClick.AddListener(() => OnSlotPressed(color));

            uiEntriesByColor[color.ToIndex()] = new CrystalUiEntry
            {
                MainLabel = mainLabel,
                LevelLabel = levelLabel,
                SlotButton = slotButton,
                SlotButtonGraphic = slotButtonGraphic,
                IndicatorImage = stateIndicator,
                IconImage = iconImage
            };
        }

        private Image EnsureSlotIcon(RectTransform slot, ManaColor color)
        {
            var iconTransform = slot.Find("CrystalIcon") as RectTransform;
            if (iconTransform == null)
            {
                var iconObject = new GameObject("CrystalIcon", typeof(RectTransform));
                iconTransform = (RectTransform)iconObject.transform;
                iconTransform.SetParent(slot, false);
            }

            iconTransform.anchorMin = new Vector2(0.5f, 0.5f);
            iconTransform.anchorMax = new Vector2(0.5f, 0.5f);
            iconTransform.anchoredPosition = new Vector2(0f, -18f);
            iconTransform.sizeDelta = new Vector2(170f, 170f);
            iconTransform.localScale = Vector3.one;

            var iconImage = iconTransform.GetComponent<Image>();
            if (iconImage == null)
            {
                iconImage = iconTransform.gameObject.AddComponent<Image>();
            }

            if (crystalConfig != null && crystalConfig.TryGetDefinition(color, out var definition) && definition != null)
            {
                iconImage.sprite = definition.IconSprite;
            }

            iconImage.type = Image.Type.Simple;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = true;
            return iconImage;
        }

        private TMP_Text EnsureManaLabel(RectTransform slot)
        {
            var labelTransform = slot.Find("ManaLabel") as RectTransform;
            if (labelTransform == null)
            {
                var labelObject = new GameObject("ManaLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelTransform = (RectTransform)labelObject.transform;
                labelTransform.SetParent(slot, false);
            }

            labelTransform.anchorMin = new Vector2(0.5f, 0.5f);
            labelTransform.anchorMax = new Vector2(0.5f, 0.5f);
            labelTransform.anchoredPosition = new Vector2(0f, -10f);
            labelTransform.sizeDelta = new Vector2(120f, 56f);
            labelTransform.localScale = Vector3.one;

            var label = labelTransform.GetComponent<TextMeshProUGUI>();
            if (label == null)
            {
                label = labelTransform.gameObject.AddComponent<TextMeshProUGUI>();
            }

            ConfigureText(label, "0", 64f, FontStyles.Bold);
            ApplyTextOutline(label, 0.42f, new Color(0f, 0f, 0f, 1f));
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            return label;
        }

        private TMP_Text EnsureLevelLabel(RectTransform slot, ManaColor color)
        {
            var levelTransform = slot.Find("LevelLabel") as RectTransform;
            if (levelTransform == null)
            {
                var levelObject = new GameObject("LevelLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
                levelTransform = (RectTransform)levelObject.transform;
                levelTransform.SetParent(slot, false);
            }

            levelTransform.anchorMin = new Vector2(0.5f, 0f);
            levelTransform.anchorMax = new Vector2(0.5f, 0f);
            levelTransform.anchoredPosition = new Vector2(0f, 20f);
            levelTransform.sizeDelta = new Vector2(260f, 34f);
            levelTransform.localScale = Vector3.one;

            var levelLabel = levelTransform.GetComponent<TextMeshProUGUI>();
            if (levelLabel == null)
            {
                levelLabel = levelTransform.gameObject.AddComponent<TextMeshProUGUI>();
            }

            ConfigureText(levelLabel, $"LV {Mathf.Max(1, GetCurrentLevel(color))}", 24f, FontStyles.Bold);
            ApplyTextOutline(levelLabel, 0.15f, new Color(0f, 0f, 0f, 1f));
            levelLabel.alignment = TextAlignmentOptions.Center;
            levelLabel.textWrappingMode = TextWrappingModes.NoWrap;
            return levelLabel;
        }

        private static void DisableLevelLabel(RectTransform slot)
        {
            var levelTransform = slot.Find("LevelLabel");
            if (levelTransform != null)
            {
                levelTransform.gameObject.SetActive(false);
            }
        }

        private static void DisableStatusLabel(RectTransform slot)
        {
            var statusTransform = slot.Find("StatusLabel");
            if (statusTransform != null)
            {
                statusTransform.gameObject.SetActive(false);
            }
        }

        private static void HideOldActionButton(RectTransform slot)
        {
            var actionTransform = slot.Find("ActionButton");
            if (actionTransform != null)
            {
                actionTransform.gameObject.SetActive(false);
            }
        }

        private Button EnsureSlotButton(RectTransform slot, Image iconImage, out Image slotButtonGraphic)
        {
            slotButtonGraphic = iconImage;
            if (slotButtonGraphic == null)
            {
                slotButtonGraphic = slot.GetComponentInChildren<Image>();
                if (slotButtonGraphic == null)
                {
                    return null;
                }
            }

            slotButtonGraphic.raycastTarget = true;

            var iconTransform = slotButtonGraphic.rectTransform;

            var slotButton = slot.GetComponent<Button>();
            if (slotButton != null)
            {
                Object.Destroy(slotButton);
            }

            if (slot.TryGetComponent<Image>(out var slotImage))
            {
                slotImage.raycastTarget = false;
                if (slotImage.sprite == null)
                {
                    Object.Destroy(slotImage);
                }
            }

            var button = iconTransform.GetComponent<Button>();
            if (button == null)
            {
                button = iconTransform.gameObject.AddComponent<Button>();
            }

            button.targetGraphic = slotButtonGraphic;
            button.transition = Selectable.Transition.ColorTint;
            button.colors = ColorBlock.defaultColorBlock;
            return button;
        }

        private static Image EnsureStateIndicator(RectTransform slot, Image iconImage)
        {
            var indicatorTransform = (iconImage != null ? iconImage.rectTransform.Find("StateIndicator") : null) as RectTransform;
            if (indicatorTransform == null)
            {
                indicatorTransform = slot.Find("StateIndicator") as RectTransform;
            }
            if (indicatorTransform == null)
            {
                var indicatorObject = new GameObject("StateIndicator", typeof(RectTransform), typeof(Image));
                indicatorTransform = (RectTransform)indicatorObject.transform;
                indicatorTransform.SetParent(slot, false);
            }

            var indicatorParent = iconImage != null ? iconImage.rectTransform : slot;
            if (indicatorTransform.parent != indicatorParent)
            {
                indicatorTransform.SetParent(indicatorParent, false);
            }

            indicatorTransform.anchorMin = new Vector2(1f, 1f);
            indicatorTransform.anchorMax = new Vector2(1f, 1f);
            indicatorTransform.pivot = new Vector2(1f, 1f);
            indicatorTransform.anchoredPosition = new Vector2(-8f, -8f);
            indicatorTransform.sizeDelta = new Vector2(42f, 42f);
            indicatorTransform.localScale = Vector3.one;

            var indicatorImage = indicatorTransform.GetComponent<Image>();
            if (indicatorImage == null)
            {
                indicatorImage = indicatorTransform.gameObject.AddComponent<Image>();
            }

            indicatorImage.preserveAspect = true;
            indicatorImage.raycastTarget = false;
            indicatorImage.gameObject.SetActive(false);
            return indicatorImage;
        }
        private void ConfigureText(TextMeshProUGUI text, string value, float size, FontStyles style)
        {
            if (text == null)
            {
                return;
            }

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

        private static void ApplyTextOutline(TextMeshProUGUI text, float width, Color color)
        {
            if (text == null)
            {
                return;
            }

            text.outlineWidth = Mathf.Clamp01(width);
            text.outlineColor = color;

            var material = text.fontMaterial;
            if (material != null)
            {
                material.EnableKeyword("OUTLINE_ON");

                if (material.HasProperty(ShaderUtilities.ID_OutlineWidth))
                {
                    material.SetFloat(ShaderUtilities.ID_OutlineWidth, Mathf.Clamp01(width));
                }

                if (material.HasProperty(ShaderUtilities.ID_OutlineColor))
                {
                    material.SetColor(ShaderUtilities.ID_OutlineColor, color);
                }

                text.fontMaterial = material;
            }

            text.UpdateMeshPadding();
            text.SetMaterialDirty();
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

        private bool CanAffordOrFree(int cost)
        {
            return cost <= 0 || progressionRuntimeManager.CanAfford(cost);
        }

        private void OpenUnlockPopup(ManaColor color, CrystalPrototypeConfig.BaseCrystalDefinition definition)
        {
            CloseActivePopup();
            PauseGameplayForPopup();

            var panel = CreatePopupPanel(out var overlayButton);
            if (panel == null)
            {
                UnpauseGameplayAfterPopup();
                return;
            }

            overlayButton.onClick.RemoveAllListeners();
            overlayButton.onClick.AddListener(CloseActivePopup);

            CreatePopupIcon(panel, definition.IconSprite);
            var costText = CreatePopupText(panel, "UnlockCostLabel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-180f, -20f), new Vector2(180f, 40f), 44f);
            costText.text = $"Cost {Mathf.Max(0, definition.UnlockCost)} XP";

            var unlockButton = CreatePopupActionButton(panel, mainUiProvider != null ? mainUiProvider.UnlockPopupButtonSprite : null, "Unlock", new Vector2(-170f, 48f), new Vector2(170f, 138f));
            unlockButton.interactable = true;
            unlockButton.onClick.AddListener(() =>
            {
                TryUnlock(color);
                CloseActivePopup();
                RefreshActionUi();
            });
        }

        private void OpenUpgradePopup(ManaColor color, CrystalPrototypeConfig.BaseCrystalDefinition definition)
        {
            CloseActivePopup();
            PauseGameplayForPopup();

            var panel = CreatePopupPanel(out var overlayButton);
            if (panel == null)
            {
                UnpauseGameplayAfterPopup();
                return;
            }

            overlayButton.onClick.RemoveAllListeners();
            overlayButton.onClick.AddListener(CloseActivePopup);

            CreatePopupIcon(panel, definition.IconSprite);

            var detailsText = CreatePopupText(panel, "UpgradeDetailsLabel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-220f, -86f), new Vector2(220f, 34f), 36f);
            detailsText.alignment = TextAlignmentOptions.Center;
            detailsText.textWrappingMode = TextWrappingModes.Normal;

            var priceText = CreatePopupText(panel, "UpgradeCostLabel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-180f, -150f), new Vector2(180f, -90f), 40f);

            var upgradeButton = CreatePopupActionButton(panel, mainUiProvider != null ? mainUiProvider.UpgradePopupButtonSprite : null, "Upgrade", new Vector2(-170f, 48f), new Vector2(170f, 138f));

            var currentLevel = Mathf.Max(1, GetCurrentLevel(color));
            var hasNextLevel = definition.Levels != null && currentLevel < definition.Levels.Length;
            var currentData = hasNextLevel && currentLevel - 1 >= 0 ? definition.Levels[currentLevel - 1] : ResolveLevelData(color);
            var nextData = hasNextLevel ? definition.Levels[currentLevel] : null;

            if (hasNextLevel && currentData != null && nextData != null)
            {
                detailsText.text = BuildUpgradeDetails(color, currentData, nextData);
                var upgradeCost = currentData.UpgradeCost;
                priceText.text = $"Cost {Mathf.Max(0, upgradeCost)} XP";
                upgradeButton.interactable = CanAffordOrFree(upgradeCost);
                upgradeButton.onClick.AddListener(() =>
                {
                    TryUpgrade(color);
                    CloseActivePopup();
                    RefreshActionUi();
                });
            }
            else
            {
                detailsText.text = "MAX LEVEL";
                priceText.text = "Cost 0 XP";
                upgradeButton.interactable = false;
            }
        }

        private static string BuildUpgradeDetails(ManaColor color, CrystalPrototypeConfig.CrystalLevelData currentData, CrystalPrototypeConfig.CrystalLevelData nextData)
        {
            var builder = new StringBuilder();
            var valueColor = GetCrystalValueColorTag(color);
            builder.Append($"Damage {valueColor}{currentData.Damage}</color> -> {valueColor}{nextData.Damage}</color>\n");
            builder.Append($"Regen {valueColor}{currentData.GenerationPerSecond:0.##}</color> -> {valueColor}{nextData.GenerationPerSecond:0.##}</color>\n");
            builder.Append($"Cap {valueColor}{currentData.ManaCap}</color> -> {valueColor}{nextData.ManaCap}</color>");
            return builder.ToString();
        }

        private static string GetCrystalValueColorTag(ManaColor color)
        {
            return color switch
            {
                ManaColor.Red => "<color=#F25656>",
                ManaColor.Green => "<color=#62D46A>",
                ManaColor.Blue => "<color=#64A6FF>",
                ManaColor.Yellow => "<color=#E3C94D>",
                ManaColor.Magenta => "<color=#D66AE8>",
                ManaColor.Cyan => "<color=#61D5E5>",
                ManaColor.White => "<color=#F2EEE0>",
                _ => "<color=#FFFFFF>"
            };
        }

        private RectTransform CreatePopupPanel(out Button overlayButton)
        {
            overlayButton = null;
            if (mainUiProvider == null || mainUiProvider.PopupParent == null)
            {
                return null;
            }

            var overlayObject = new GameObject("CrystalActionPopupOverlay", typeof(RectTransform), typeof(Image), typeof(Button));
            activePopupOverlay = (RectTransform)overlayObject.transform;
            activePopupOverlay.SetParent(mainUiProvider.PopupParent, false);
            activePopupOverlay.anchorMin = Vector2.zero;
            activePopupOverlay.anchorMax = Vector2.one;
            activePopupOverlay.offsetMin = Vector2.zero;
            activePopupOverlay.offsetMax = Vector2.zero;
            activePopupOverlay.localScale = Vector3.one;

            var overlayImage = overlayObject.GetComponent<Image>();
            overlayImage.color = new Color(0f, 0f, 0f, 0.7f);
            overlayButton = overlayObject.GetComponent<Button>();
            overlayButton.targetGraphic = overlayImage;

            var panelObject = new GameObject("CrystalActionPopupPanel", typeof(RectTransform), typeof(Image));
            var panel = (RectTransform)panelObject.transform;
            panel.SetParent(activePopupOverlay, false);
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(520f, 620f);
            panel.anchoredPosition = Vector2.zero;
            panel.localScale = Vector3.one * 1.5f;

            var panelImage = panelObject.GetComponent<Image>();
            panelImage.sprite = mainUiProvider != null ? mainUiProvider.PopupBackgroundSprite : null;
            panelImage.color = panelImage.sprite != null ? Color.white : new Color(0.23f, 0.15f, 0.1f, 0.96f);
            panelImage.type = Image.Type.Sliced;
            panelImage.fillCenter = true;
            panelImage.pixelsPerUnitMultiplier = 2.5f;
            panelImage.raycastTarget = true;

            return panel;
        }

        private static void CreatePopupIcon(RectTransform panel, Sprite iconSprite)
        {
            var iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            var iconRect = (RectTransform)iconObject.transform;
            iconRect.SetParent(panel, false);
            iconRect.anchorMin = new Vector2(0.5f, 1f);
            iconRect.anchorMax = new Vector2(0.5f, 1f);
            iconRect.pivot = new Vector2(0.5f, 1f);
            iconRect.sizeDelta = new Vector2(220f, 220f);
            iconRect.anchoredPosition = new Vector2(0f, -26f);
            iconRect.localScale = Vector3.one;

            var iconImage = iconObject.GetComponent<Image>();
            iconImage.sprite = iconSprite;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
        }

        private TMP_Text CreatePopupText(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, float fontSize)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            var rect = (RectTransform)textObject.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            rect.localScale = Vector3.one;

            var text = textObject.GetComponent<TextMeshProUGUI>();
            ConfigureText(text, string.Empty, fontSize, FontStyles.Bold);
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(1f, 0.92f, 0.8f, 1f);
            return text;
        }

        private Button CreatePopupActionButton(RectTransform panel, Sprite buttonSprite, string label, Vector2 offsetMin, Vector2 offsetMax)
        {
            var buttonObject = new GameObject("ActionButton", typeof(RectTransform), typeof(Image), typeof(Button));
            var buttonRect = (RectTransform)buttonObject.transform;
            buttonRect.SetParent(panel, false);
            buttonRect.anchorMin = new Vector2(0.5f, 0f);
            buttonRect.anchorMax = new Vector2(0.5f, 0f);
            buttonRect.pivot = new Vector2(0.5f, 0f);
            buttonRect.offsetMin = offsetMin;
            buttonRect.offsetMax = offsetMax;
            buttonRect.localScale = Vector3.one;

            var image = buttonObject.GetComponent<Image>();
            image.sprite = buttonSprite;
            image.type = Image.Type.Sliced;
            image.fillCenter = true;
            image.pixelsPerUnitMultiplier = 2.5f;
            image.preserveAspect = false;
            image.color = Color.white;

            var button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            var labelRect = (RectTransform)labelObject.transform;
            labelRect.SetParent(buttonRect, false);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            labelRect.localScale = Vector3.one;

            var labelText = labelObject.GetComponent<TextMeshProUGUI>();
            ConfigureText(labelText, label, 44f, FontStyles.Bold);
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = new Color(0.18f, 0.12f, 0.08f, 1f);

            return button;
        }

        private void PauseGameplayForPopup()
        {
            if (isPopupOpen)
            {
                return;
            }

            storedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            isPopupOpen = true;
        }

        private void UnpauseGameplayAfterPopup()
        {
            if (!isPopupOpen)
            {
                return;
            }

            Time.timeScale = storedTimeScale;
            isPopupOpen = false;
        }

        private void CloseActivePopup()
        {
            if (activePopupOverlay != null)
            {
                Object.Destroy(activePopupOverlay.gameObject);
                activePopupOverlay = null;
            }

            UnpauseGameplayAfterPopup();
        }

        private sealed class CrystalUiEntry
        {
            public TMP_Text MainLabel;
            public TMP_Text LevelLabel;
            public Button SlotButton;
            public Image SlotButtonGraphic;
            public Image IndicatorImage;
            public Image IconImage;
        }
    }
}


