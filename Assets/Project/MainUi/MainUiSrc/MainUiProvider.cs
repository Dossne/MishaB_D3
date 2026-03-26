using System;
using System.Collections.Generic;
using RainbowTower.ManaSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RainbowTower.MainUi
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public sealed class MainUiProvider : MonoBehaviour
    {
        private const float ReferenceWidth = 1080f;
        private const float ReferenceHeight = 1920f;
        private const float OuterMargin = 36f;
        private const float TopHudHeight = 150f;
        private const float GameFieldHeight = 1080f;
        private const float ShelfZoneHeight = ReferenceHeight - GameFieldHeight - TopHudHeight;
        private const float ShelfSpacing = 12f;
        private const float CrystalShelfPanelHeight = 755f;
        private const float CrystalSpendTextLifetime = 0.55f;
        private const float CrystalSpendTextRiseDistance = 42f;

        [Header("Font")]
        [SerializeField] private TMP_FontAsset uiFontAsset;

        [Header("Roots")]
        [SerializeField] private RectTransform floatingTextParent;
        [SerializeField] private RectTransform hudParent;
        [SerializeField] private RectTransform popupParent;

        [Header("Hud")]
        [SerializeField] private TMP_Text hpLabel;
        [SerializeField] private TMP_Text waveLabel;

        [Header("Crystal Panel")]
        [SerializeField] private TMP_Text redCrystalLabel;
        [SerializeField] private TMP_Text greenCrystalLabel;
        [SerializeField] private TMP_Text blueCrystalLabel;
        [SerializeField] private TMP_Text yellowCrystalLabel;
        [SerializeField] private TMP_Text magentaCrystalLabel;
        [SerializeField] private TMP_Text cyanCrystalLabel;
        [SerializeField] private TMP_Text whiteCrystalLabel;
        [SerializeField] private Sprite crystalShelfBackgroundSprite;
        [SerializeField] private Sprite redCrystalButtonSprite;
        [SerializeField] private Sprite greenCrystalButtonSprite;
        [SerializeField] private Sprite blueCrystalButtonSprite;
        [SerializeField] private Sprite yellowCrystalButtonSprite;
        [SerializeField] private Sprite magentaCrystalButtonSprite;
        [SerializeField] private Sprite cyanCrystalButtonSprite;
        [SerializeField] private Sprite whiteCrystalButtonSprite;
        [SerializeField] private Sprite unlockPopupButtonSprite;
        [SerializeField] private Sprite upgradePopupButtonSprite;
        [SerializeField] private Sprite lockIndicatorSprite;
        [SerializeField] private Sprite upgradeIndicatorSprite;
        [SerializeField] private Sprite popupBackgroundSprite;
        [SerializeField] private RectTransform crystalSlotPrototype;
        [SerializeField] private Button unlockAllCrystalsCheatButton;

        [Header("Defeat Popup")]
        [SerializeField] private RectTransform defeatPopupRoot;
        [SerializeField] private TMP_Text defeatTitleLabel;
        [SerializeField] private TMP_Text defeatMessageLabel;
        [SerializeField] private Button restartButton;
        [SerializeField] private TMP_Text restartButtonLabel;

        private readonly List<CrystalSpendFloatingTextEntry> activeCrystalSpendTexts = new();

        public RectTransform FloatingTextParent => floatingTextParent;
        public RectTransform HudParent => hudParent;
        public RectTransform PopupParent => popupParent;
        public TMP_Text HpLabel => hpLabel;
        public TMP_Text WaveLabel => waveLabel;
        public Sprite UnlockPopupButtonSprite => unlockPopupButtonSprite;
        public Sprite UpgradePopupButtonSprite => upgradePopupButtonSprite;
        public Sprite LockIndicatorSprite => lockIndicatorSprite;
        public Sprite UpgradeIndicatorSprite => upgradeIndicatorSprite;
        public Sprite PopupBackgroundSprite => popupBackgroundSprite;

        public void SetHudValues(int currentHp, int maxHp, int currentWave, int totalWaves)
        {
            if (hpLabel != null)
            {
                hpLabel.text = $"HP {currentHp}/{maxHp}";
            }

            if (waveLabel != null)
            {
                waveLabel.text = $"Wave {currentWave}/{totalWaves}";
            }
        }

        public void SetBaseCrystalPanelValues(int redMana, int redLevel, int greenMana, int greenLevel, int blueMana, int blueLevel)
        {
            if (redCrystalLabel != null)
            {
                redCrystalLabel.text = $"{Mathf.Max(0, redMana)}";
            }

            if (greenCrystalLabel != null)
            {
                greenCrystalLabel.text = $"{Mathf.Max(0, greenMana)}";
            }

            if (blueCrystalLabel != null)
            {
                blueCrystalLabel.text = $"{Mathf.Max(0, blueMana)}";
            }
        }

        public void SetMixedCrystalPanelValues(int yellowMana, int yellowLevel, int magentaMana, int magentaLevel, int cyanMana, int cyanLevel)
        {
            if (yellowCrystalLabel != null)
            {
                yellowCrystalLabel.text = $"{Mathf.Max(0, yellowMana)}";
            }

            if (magentaCrystalLabel != null)
            {
                magentaCrystalLabel.text = $"{Mathf.Max(0, magentaMana)}";
            }

            if (cyanCrystalLabel != null)
            {
                cyanCrystalLabel.text = $"{Mathf.Max(0, cyanMana)}";
            }
        }

        public void SetWhiteCrystalPanelValues(int whiteMana, int whiteLevel)
        {
            if (whiteCrystalLabel != null)
            {
                whiteCrystalLabel.text = $"{Mathf.Max(0, whiteMana)}";
            }
        }
        public void ShowCrystalSpendFloatingText(ManaColor color, int amount)
        {
            if (amount <= 0 || floatingTextParent == null)
            {
                return;
            }

            var crystalLabel = GetCrystalLabelByColor(color);
            if (crystalLabel == null)
            {
                return;
            }

            var textObject = new GameObject("CrystalSpendText", typeof(RectTransform), typeof(TextMeshProUGUI));
            var textRect = (RectTransform)textObject.transform;
            textRect.SetParent(floatingTextParent, false);
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.localScale = Vector3.one;
            textRect.sizeDelta = new Vector2(220f, 52f);

            var localStartPosition = floatingTextParent.InverseTransformPoint(crystalLabel.rectTransform.position);
            textRect.anchoredPosition = new Vector2(localStartPosition.x, localStartPosition.y + 10f);

            var text = textObject.GetComponent<TextMeshProUGUI>();
            text.font = uiFontAsset != null ? uiFontAsset : TMP_Settings.defaultFontAsset;
            text.fontSize = 28f;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.text = $"-{amount}";
            text.color = new Color(1f, 0.78f, 0.4f, 1f);
            text.raycastTarget = false;

            activeCrystalSpendTexts.Add(new CrystalSpendFloatingTextEntry
            {
                Text = text,
                StartPosition = textRect.anchoredPosition,
                EndPosition = textRect.anchoredPosition + new Vector2(0f, CrystalSpendTextRiseDistance),
                RemainingLifetime = CrystalSpendTextLifetime,
                TotalLifetime = CrystalSpendTextLifetime
            });
        }

        public void BindUnlockAllCrystalsCheat(Action onClick)
        {
            if (unlockAllCrystalsCheatButton == null)
            {
                return;
            }

            unlockAllCrystalsCheatButton.onClick.RemoveAllListeners();
            if (onClick != null)
            {
                unlockAllCrystalsCheatButton.onClick.AddListener(() => onClick.Invoke());
            }
        }

        public void ShowDefeatPopup(Action onRestart)
        {
            ShowSessionPopup(
                "Defeat",
                "The tower was overwhelmed.",
                new Color(1f, 0.78f, 0.42f, 1f),
                onRestart);
        }

        public void ShowVictoryPopup(string message, Action onRestart)
        {
            ShowSessionPopup(
                "Milestone Reached",
                string.IsNullOrWhiteSpace(message) ? "You survived the session prototype." : message,
                new Color(0.7f, 1f, 0.75f, 1f),
                onRestart);
        }

        private void ShowSessionPopup(string title, string message, Color titleColor, Action onRestart)
        {
            EnsureEventSystem();
            EnsureDefeatPopup();
            if (defeatPopupRoot == null)
            {
                return;
            }

            defeatPopupRoot.gameObject.SetActive(true);

            if (defeatTitleLabel != null)
            {
                defeatTitleLabel.text = title;
                defeatTitleLabel.color = titleColor;
            }

            if (defeatMessageLabel != null)
            {
                defeatMessageLabel.text = message;
            }

            if (restartButtonLabel != null)
            {
                restartButtonLabel.text = "Restart";
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(() =>
                {
                    onRestart?.Invoke();
                });
            }
        }

        public void HideDefeatPopup()
        {
            if (defeatPopupRoot != null)
            {
                defeatPopupRoot.gameObject.SetActive(false);
            }
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            EnsureCrystalSpritesAssignedInEditor();
#endif
        }

#if UNITY_EDITOR
        private void EnsureCrystalSpritesAssignedInEditor()
        {
            crystalShelfBackgroundSprite = LoadSpriteIfMissing(crystalShelfBackgroundSprite, "Assets/Project/GameplayField/GameplayFieldArt/shelf.png");
            redCrystalButtonSprite = LoadSpriteIfMissing(redCrystalButtonSprite, "Assets/Project/CrystalSystem/CrystalSystemArt/mana_crystal_red.png");
            greenCrystalButtonSprite = LoadSpriteIfMissing(greenCrystalButtonSprite, "Assets/Project/CrystalSystem/CrystalSystemArt/mana_crystal_green.png");
            blueCrystalButtonSprite = LoadSpriteIfMissing(blueCrystalButtonSprite, "Assets/Project/CrystalSystem/CrystalSystemArt/mana_crystal_blue.png");
            yellowCrystalButtonSprite = LoadSpriteIfMissing(yellowCrystalButtonSprite, "Assets/Project/CrystalSystem/CrystalSystemArt/mana_crystal_yellow.png");
            magentaCrystalButtonSprite = LoadSpriteIfMissing(magentaCrystalButtonSprite, "Assets/Project/CrystalSystem/CrystalSystemArt/mana_crystal_magenta.png");
            cyanCrystalButtonSprite = LoadSpriteIfMissing(cyanCrystalButtonSprite, "Assets/Project/CrystalSystem/CrystalSystemArt/mana_crystal_cyan.png");
            whiteCrystalButtonSprite = LoadSpriteIfMissing(whiteCrystalButtonSprite, "Assets/Project/CrystalSystem/CrystalSystemArt/mana_crystal_white.png");
            unlockPopupButtonSprite = LoadSpriteIfMissing(unlockPopupButtonSprite, "Assets/Project/MainUi/MainUiArt/button_gold_plain.png");
            upgradePopupButtonSprite = LoadSpriteIfMissing(upgradePopupButtonSprite, "Assets/Project/MainUi/MainUiArt/button_green_plain.png");
            lockIndicatorSprite = LoadSpriteIfMissing(lockIndicatorSprite, "Assets/Project/MainUi/MainUiArt/lock_fantasy_gold.png");
            upgradeIndicatorSprite = LoadSpriteIfMissing(upgradeIndicatorSprite, "Assets/Project/MainUi/MainUiArt/button_arrow_up_gold.png");
            popupBackgroundSprite = LoadSpriteIfMissing(popupBackgroundSprite, "Assets/Project/MainUi/MainUiArt/popup_fantasy_bg.png");
            crystalSlotPrototype = LoadRectTransformIfMissing(crystalSlotPrototype, "Assets/Project/MainUi/MainUiPfs/CrystalShelfSlot.prefab");
        }

        private static RectTransform LoadRectTransformIfMissing(RectTransform current, string assetPath)
        {
            return current != null ? current : AssetDatabase.LoadAssetAtPath<RectTransform>(assetPath);
        }

        private static Sprite LoadSpriteIfMissing(Sprite current, string assetPath)
        {
            return current != null ? current : AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }
#endif

        private void Awake()
        {
#if UNITY_EDITOR
            EnsureCrystalSpritesAssignedInEditor();
#endif
            EnsureCanvasSetup();
            EnsureEventSystem();
            EnsureHierarchy();
            HideDefeatPopup();
        }

        private void Update()
        {
            TickCrystalSpendFloatingTexts(Time.unscaledDeltaTime);
        }

        private void EnsureCanvasSetup()
        {
            var root = (RectTransform)transform;
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;
            root.pivot = new Vector2(0.5f, 0.5f);
            root.localScale = Vector3.one;
            root.localPosition = Vector3.zero;

            var canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;

            var scaler = GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        private void EnsureHierarchy()
        {
            floatingTextParent = EnsureChildRoot(floatingTextParent, "FloatingTextParent");
            hudParent = EnsureChildRoot(hudParent, "HudParent");
            popupParent = EnsureChildRoot(popupParent, "PopupParent");

            hudParent.SetSiblingIndex(0);
            floatingTextParent.SetSiblingIndex(1);
            popupParent.SetSiblingIndex(2);

            BuildHud();
            BuildCrystalShelf();
            AssignCrystalShelfReferences();
            SetBaseCrystalPanelValues(0, 1, 0, 1, 0, 1);
            SetMixedCrystalPanelValues(0, 1, 0, 1, 0, 1);
            SetWhiteCrystalPanelValues(0, 1);
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            DontDestroyOnLoad(eventSystemObject);
        }

        private RectTransform EnsureChildRoot(RectTransform existingRoot, string rootName)
        {
            if (existingRoot != null)
            {
                return existingRoot;
            }

            var rootObject = new GameObject(rootName, typeof(RectTransform));
            var rootTransform = (RectTransform)rootObject.transform;
            rootTransform.SetParent(transform, false);
            rootTransform.anchorMin = Vector2.zero;
            rootTransform.anchorMax = Vector2.one;
            rootTransform.offsetMin = Vector2.zero;
            rootTransform.offsetMax = Vector2.zero;
            rootTransform.localScale = Vector3.one;
            return rootTransform;
        }

        private void BuildHud()
        {
            var existingTopHudPanel = hudParent.Find("TopHudPanel") as RectTransform;
            if (existingTopHudPanel != null)
            {
                Destroy(existingTopHudPanel.gameObject);
            }

            var topHudPanel = CreatePanel(
                "TopHudPanel",
                hudParent,
                new Color(0f, 0f, 0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(OuterMargin, -(OuterMargin + TopHudHeight)),
                new Vector2(-OuterMargin, -OuterMargin));

            var layoutGroup = topHudPanel.gameObject.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.padding = new RectOffset(0, 0, 12, 12);
            layoutGroup.spacing = 20f;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = true;

            var wavePanel = CreateLayoutPanel("WavePanel", topHudPanel, new Color(0.95f, 0.68f, 0.12f, 0.95f), 1f, 0f);
            waveLabel = CreateStretchText("WaveLabel", wavePanel, "Wave 1/10", 52f);
            waveLabel.color = new Color(0.98f, 0.97f, 0.88f, 1f);

            var xpPanel = CreateLayoutPanel("XpPanel", topHudPanel, new Color(0.74f, 0.41f, 0.11f, 0.95f), 1f, 0f);
            var xpTopLabel = CreateStretchText("XpLabel", xpPanel, "XP 0", 46f);
            xpTopLabel.color = new Color(1f, 0.95f, 0.84f, 1f);

            var hpPanel = CreateLayoutPanel("HpPanel", topHudPanel, new Color(0.63f, 0.32f, 0.08f, 0.95f), 1f, 0f);
            hpLabel = CreateStretchText("HpLabel", hpPanel, "HP 25/30", 46f);
            hpLabel.color = new Color(1f, 0.95f, 0.84f, 1f);

            AssignTopHudReferences();
        }

        private void BuildCrystalShelf()
        {
            var existingShelfPanel = hudParent.Find("CrystalShelfPanel") as RectTransform;
            if (existingShelfPanel != null)
            {
                var existingTitle = existingShelfPanel.Find("ShelfTitle");
                if (existingTitle != null)
                {
                    Destroy(existingTitle.gameObject);
                }

                var existingCheat = existingShelfPanel.Find("UnlockAllCheatButton");
                if (existingCheat != null)
                {
                    Destroy(existingCheat.gameObject);
                }

                ApplyCrystalShelfLayout(existingShelfPanel);
                ApplyCrystalShelfBackground(existingShelfPanel);
                RebuildCrystalRows(existingShelfPanel);
                unlockAllCrystalsCheatButton = null;
                return;
            }

            var shelfPanel = CreatePanel(
                "CrystalShelfPanel",
                hudParent,
                new Color(0.34f, 0.2f, 0.1f, 0.96f),
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(OuterMargin, OuterMargin),
                new Vector2(-OuterMargin, OuterMargin + ShelfZoneHeight));
            ApplyCrystalShelfLayout(shelfPanel);
            ApplyCrystalShelfBackground(shelfPanel);
            unlockAllCrystalsCheatButton = null;

            var rowsObject = new GameObject("ShelfRows", typeof(RectTransform));
            var rowsTransform = (RectTransform)rowsObject.transform;
            rowsTransform.SetParent(shelfPanel, false);
            rowsTransform.anchorMin = new Vector2(0f, 0f);
            rowsTransform.anchorMax = new Vector2(1f, 1f);
            rowsTransform.offsetMin = new Vector2(20f, 80f);
            rowsTransform.offsetMax = new Vector2(-20f, -70f);
            rowsTransform.localScale = Vector3.one;

            var rowsLayout = rowsTransform.gameObject.AddComponent<VerticalLayoutGroup>();
            rowsLayout.padding = new RectOffset(0, 0, 0, 0);
            rowsLayout.spacing = 0f;
            rowsLayout.childAlignment = TextAnchor.UpperCenter;
            rowsLayout.reverseArrangement = false;
            rowsLayout.childControlWidth = true;
            rowsLayout.childControlHeight = true;
            rowsLayout.childScaleWidth = false;
            rowsLayout.childScaleHeight = false;
            rowsLayout.childForceExpandWidth = true;
            rowsLayout.childForceExpandHeight = true;

            CreateCrystalRow("TopRow", rowsTransform, "Red", "Green", "Blue");
            CreateCrystalRow("MiddleRow", rowsTransform, "Yellow", "Magenta", "Cyan");
            CreateCrystalRow("BottomRow", rowsTransform, "White");
        }

        private void RebuildCrystalRows(RectTransform shelfPanel)
        {
            var existingRows = shelfPanel.Find("ShelfRows") as RectTransform;
            if (existingRows != null)
            {
                Destroy(existingRows.gameObject);
            }

            var rowsObject = new GameObject("ShelfRows", typeof(RectTransform));
            var rowsTransform = (RectTransform)rowsObject.transform;
            rowsTransform.SetParent(shelfPanel, false);
            rowsTransform.anchorMin = new Vector2(0f, 0f);
            rowsTransform.anchorMax = new Vector2(1f, 1f);
            rowsTransform.offsetMin = new Vector2(20f, 80f);
            rowsTransform.offsetMax = new Vector2(-20f, -70f);
            rowsTransform.localScale = Vector3.one;

            var rowsLayout = rowsTransform.gameObject.AddComponent<VerticalLayoutGroup>();
            rowsLayout.padding = new RectOffset(0, 0, 0, 0);
            rowsLayout.spacing = 0f;
            rowsLayout.childAlignment = TextAnchor.UpperCenter;
            rowsLayout.reverseArrangement = false;
            rowsLayout.childControlWidth = true;
            rowsLayout.childControlHeight = true;
            rowsLayout.childScaleWidth = false;
            rowsLayout.childScaleHeight = false;
            rowsLayout.childForceExpandWidth = true;
            rowsLayout.childForceExpandHeight = true;

            CreateCrystalRow("TopRow", rowsTransform, "Red", "Green", "Blue");
            CreateCrystalRow("MiddleRow", rowsTransform, "Yellow", "Magenta", "Cyan");
            CreateCrystalRow("BottomRow", rowsTransform, "White");
        }

        private void EnsureDefeatPopup()
        {
            if (defeatPopupRoot != null)
            {
                return;
            }

            defeatPopupRoot = CreatePanel(
                "DefeatPopup",
                popupParent,
                new Color(0f, 0f, 0f, 0.74f),
                new Vector2(0f, 0f),
                new Vector2(1f, 1f),
                Vector2.zero,
                Vector2.zero);

            var window = CreatePanel(
                "Window",
                defeatPopupRoot,
                new Color(0.27f, 0.16f, 0.12f, 0.96f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-300f, -180f),
                new Vector2(300f, 180f));

            if (popupBackgroundSprite != null && window.TryGetComponent<Image>(out var windowImage))
            {
                windowImage.sprite = popupBackgroundSprite;
                windowImage.color = Color.white;
                windowImage.type = Image.Type.Sliced;
                windowImage.fillCenter = true;
                windowImage.pixelsPerUnitMultiplier = 2.5f;
            }

            defeatTitleLabel = CreateAnchoredText(
                "DefeatLabel",
                window,
                "Defeat",
                70f,
                new Vector2(0f, 0.58f),
                new Vector2(1f, 1f),
                new Vector2(30f, -124f),
                new Vector2(-30f, -24f));
            defeatTitleLabel.color = new Color(1f, 0.78f, 0.42f, 1f);

            defeatMessageLabel = CreateAnchoredText(
                "MessageLabel",
                window,
                "The tower was overwhelmed.",
                36f,
                new Vector2(0f, 0.35f),
                new Vector2(1f, 0.7f),
                new Vector2(36f, -24f),
                new Vector2(-36f, -24f));
            defeatMessageLabel.color = new Color(1f, 0.93f, 0.82f, 1f);
            defeatMessageLabel.textWrappingMode = TextWrappingModes.Normal;

            var buttonTransform = CreatePanel(
                "RestartButton",
                window,
                new Color(0.92f, 0.56f, 0.2f, 1f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(-190f, 30f),
                new Vector2(190f, 112f));

            restartButton = buttonTransform.gameObject.AddComponent<Button>();
            restartButtonLabel = CreateAnchoredText(
                "Label",
                buttonTransform,
                "Restart",
                52f,
                Vector2.zero,
                Vector2.one,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f));
            restartButtonLabel.color = new Color(0.24f, 0.13f, 0.08f, 1f);

            defeatPopupRoot.gameObject.SetActive(false);
        }

        private void ApplyCrystalShelfLayout(RectTransform shelfPanel)
        {
            if (shelfPanel == null)
            {
                return;
            }

            shelfPanel.anchorMin = new Vector2(0f, 0f);
            shelfPanel.anchorMax = new Vector2(1f, 0f);
            shelfPanel.pivot = new Vector2(0.5f, 0f);
            shelfPanel.offsetMin = new Vector2(0f, 0f);
            shelfPanel.offsetMax = new Vector2(0f, CrystalShelfPanelHeight);
            shelfPanel.localScale = Vector3.one;
            shelfPanel.localRotation = Quaternion.identity;
        }

        private void ApplyCrystalShelfBackground(RectTransform shelfPanel)
        {
            if (shelfPanel == null)
            {
                return;
            }

            var image = shelfPanel.GetComponent<Image>();
            if (image == null || crystalShelfBackgroundSprite == null)
            {
                return;
            }

            image.sprite = crystalShelfBackgroundSprite;
            image.color = Color.white;
            image.type = Image.Type.Simple;
        }

        private void AssignTopHudReferences()
        {
            hpLabel = FindText(hudParent, "HpLabel");
            waveLabel = FindText(hudParent, "WaveLabel");
        }

        private void AssignCrystalShelfReferences()
        {
            redCrystalLabel = FindText(hudParent, "CrystalShelfPanel/ShelfRows/TopRow/RedSlot/ManaLabel");
            greenCrystalLabel = FindText(hudParent, "CrystalShelfPanel/ShelfRows/TopRow/GreenSlot/ManaLabel");
            blueCrystalLabel = FindText(hudParent, "CrystalShelfPanel/ShelfRows/TopRow/BlueSlot/ManaLabel");
            yellowCrystalLabel = FindText(hudParent, "CrystalShelfPanel/ShelfRows/MiddleRow/YellowSlot/ManaLabel");
            magentaCrystalLabel = FindText(hudParent, "CrystalShelfPanel/ShelfRows/MiddleRow/MagentaSlot/ManaLabel");
            cyanCrystalLabel = FindText(hudParent, "CrystalShelfPanel/ShelfRows/MiddleRow/CyanSlot/ManaLabel");
            whiteCrystalLabel = FindText(hudParent, "CrystalShelfPanel/ShelfRows/BottomRow/WhiteSlot/ManaLabel");
            unlockAllCrystalsCheatButton = null;
        }

        private RectTransform CreatePanel(
            string objectName,
            RectTransform parent,
            Color backgroundColor,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            var panelObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            var panelTransform = (RectTransform)panelObject.transform;
            panelTransform.SetParent(parent, false);
            panelTransform.anchorMin = anchorMin;
            panelTransform.anchorMax = anchorMax;
            panelTransform.offsetMin = offsetMin;
            panelTransform.offsetMax = offsetMax;
            panelTransform.localScale = Vector3.one;

            var image = panelObject.GetComponent<Image>();
            image.color = backgroundColor;

            return panelTransform;
        }

        private RectTransform CreateLayoutPanel(
            string objectName,
            RectTransform parent,
            Color backgroundColor,
            float flexibleWidth,
            float preferredWidth)
        {
            var panelTransform = CreatePanel(
                objectName,
                parent,
                backgroundColor,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);

            var layoutElement = panelTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.flexibleWidth = flexibleWidth;
            layoutElement.preferredWidth = preferredWidth;
            layoutElement.minHeight = 0f;

            return panelTransform;
        }

        private TMP_Text CreateStretchText(string objectName, RectTransform parent, string text, float fontSize)
        {
            var labelObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
            var labelTransform = (RectTransform)labelObject.transform;
            labelTransform.SetParent(parent, false);
            labelTransform.localScale = Vector3.one;

            var layoutElement = labelObject.AddComponent<LayoutElement>();
            layoutElement.flexibleWidth = 1f;
            layoutElement.flexibleHeight = 1f;

            var textComponent = ConfigureText(labelObject.GetComponent<TextMeshProUGUI>(), text, fontSize, FontStyles.Bold);
            textComponent.alignment = TextAlignmentOptions.Center;
            return textComponent;
        }

        private TMP_Text CreateAnchoredText(
            string objectName,
            RectTransform parent,
            string text,
            float fontSize,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            var labelObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
            var labelTransform = (RectTransform)labelObject.transform;
            labelTransform.SetParent(parent, false);
            labelTransform.anchorMin = anchorMin;
            labelTransform.anchorMax = anchorMax;
            labelTransform.offsetMin = offsetMin;
            labelTransform.offsetMax = offsetMax;
            labelTransform.localScale = Vector3.one;

            var textComponent = ConfigureText(labelObject.GetComponent<TextMeshProUGUI>(), text, fontSize, FontStyles.Bold);
            textComponent.alignment = TextAlignmentOptions.Center;
            return textComponent;
        }

        private void CreateCrystalRow(string objectName, RectTransform parent, params string[] crystalNames)
        {
            var rowObject = new GameObject(objectName, typeof(RectTransform));
            var rowTransform = (RectTransform)rowObject.transform;
            rowTransform.SetParent(parent, false);
            rowTransform.localScale = Vector3.one;

            var layoutElement = rowObject.AddComponent<LayoutElement>();
            layoutElement.flexibleHeight = 1f;
            layoutElement.flexibleWidth = 1f;
            layoutElement.minHeight = 0f;
            layoutElement.preferredHeight = 0f;

            var rowLayout = rowObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = ShelfSpacing;
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = true;

            for (var index = 0; index < crystalNames.Length; index++)
            {
                CreateCrystalSlot(crystalNames[index], rowTransform);
            }
        }

        private void CreateCrystalSlot(string crystalName, RectTransform parent)
        {
            RectTransform slotTransform;
            if (crystalSlotPrototype != null)
            {
                slotTransform = Instantiate(crystalSlotPrototype, parent, false);
            }
            else
            {
                var slotObject = new GameObject("CrystalShelfSlot", typeof(RectTransform));
                slotTransform = (RectTransform)slotObject.transform;
                slotTransform.SetParent(parent, false);
            }

            slotTransform.name = $"{crystalName}Slot";
            slotTransform.localScale = Vector3.one;

            var layoutElement = slotTransform.GetComponent<LayoutElement>() ?? slotTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.flexibleHeight = 1f;
            layoutElement.flexibleWidth = 1f;
            layoutElement.minHeight = 0f;
            layoutElement.preferredHeight = 0f;

            EnsureCrystalSlotPrototypeChildren(slotTransform);
        }

        private static void EnsureCrystalSlotPrototypeChildren(RectTransform slotTransform)
        {
            EnsureChildRect(slotTransform, "LevelLabel");
            EnsureChildRect(slotTransform, "CrystalIcon");
            EnsureChildRect(slotTransform, "ManaLabel");
            EnsureChildRect(slotTransform, "StatusLabel");
            var actionButton = EnsureChildRect(slotTransform, "ActionButton");
            EnsureChildRect(actionButton, "Label");
        }

        private static RectTransform EnsureChildRect(RectTransform parent, string childName)
        {
            var existing = parent.Find(childName) as RectTransform;
            if (existing != null)
            {
                return existing;
            }

            var child = new GameObject(childName, typeof(RectTransform));
            var rect = (RectTransform)child.transform;
            rect.SetParent(parent, false);
            rect.localScale = Vector3.one;
            return rect;
        }


        private TMP_Text ConfigureText(TextMeshProUGUI textComponent, string text, float fontSize, FontStyles fontStyle)
        {
            textComponent.font = uiFontAsset;
            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.fontStyle = fontStyle;
            textComponent.color = Color.white;
            textComponent.textWrappingMode = TextWrappingModes.NoWrap;
            textComponent.raycastTarget = false;
            textComponent.margin = Vector4.zero;
            return textComponent;
        }

        private void TickCrystalSpendFloatingTexts(float deltaTime)
        {
            for (var index = activeCrystalSpendTexts.Count - 1; index >= 0; index--)
            {
                var entry = activeCrystalSpendTexts[index];
                if (entry.Text == null)
                {
                    activeCrystalSpendTexts.RemoveAt(index);
                    continue;
                }

                entry.RemainingLifetime -= deltaTime;
                if (entry.RemainingLifetime <= 0f)
                {
                    Destroy(entry.Text.gameObject);
                    activeCrystalSpendTexts.RemoveAt(index);
                    continue;
                }

                var progress = 1f - entry.RemainingLifetime / entry.TotalLifetime;
                var rectTransform = entry.Text.rectTransform;
                rectTransform.anchoredPosition = Vector2.Lerp(entry.StartPosition, entry.EndPosition, progress);

                var color = entry.Text.color;
                color.a = Mathf.Lerp(1f, 0f, progress);
                entry.Text.color = color;

                activeCrystalSpendTexts[index] = entry;
            }
        }

        private TMP_Text GetCrystalLabelByColor(ManaColor color)
        {
            return color switch
            {
                ManaColor.Red => redCrystalLabel,
                ManaColor.Green => greenCrystalLabel,
                ManaColor.Blue => blueCrystalLabel,
                ManaColor.Yellow => yellowCrystalLabel,
                ManaColor.Magenta => magentaCrystalLabel,
                ManaColor.Cyan => cyanCrystalLabel,
                ManaColor.White => whiteCrystalLabel,
                _ => null
            };
        }

        private Button FindButton(RectTransform root, string childName)
        {
            var child = root.Find(childName);
            if (child != null && child.TryGetComponent<Button>(out var button))
            {
                return button;
            }

            return null;
        }

        private static RectTransform FindDescendantRectTransformByName(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            for (var index = 0; index < root.childCount; index++)
            {
                var child = root.GetChild(index);
                if (child.name.Equals(childName, StringComparison.Ordinal))
                {
                    return child as RectTransform;
                }

                var nested = FindDescendantRectTransformByName(child, childName);
                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
        }

        private TMP_Text FindText(RectTransform root, string childName)
        {
            var child = root.Find(childName);
            if (child != null && child.TryGetComponent<TMP_Text>(out var textComponent))
            {
                return textComponent;
            }

            var nestedChild = root.Find($"TopHudPanel/{childName}");
            if (nestedChild != null && nestedChild.TryGetComponent<TMP_Text>(out textComponent))
            {
                return textComponent;
            }

            var deepChild = root.Find($"TopHudPanel/WavePanel/{childName}");
            if (deepChild != null && deepChild.TryGetComponent<TMP_Text>(out textComponent))
            {
                return textComponent;
            }

            deepChild = root.Find($"TopHudPanel/HpPanel/{childName}");
            if (deepChild != null && deepChild.TryGetComponent<TMP_Text>(out textComponent))
            {
                return textComponent;
            }

            return null;
        }

        private Sprite GetCrystalButtonSprite(string crystalName)
        {
            return crystalName switch
            {
                "Red" => redCrystalButtonSprite,
                "Green" => greenCrystalButtonSprite,
                "Blue" => blueCrystalButtonSprite,
                "Yellow" => yellowCrystalButtonSprite,
                "Magenta" => magentaCrystalButtonSprite,
                "Cyan" => cyanCrystalButtonSprite,
                "White" => whiteCrystalButtonSprite,
                _ => null
            };
        }

        private static string FormatBaseCrystalLabel(string crystalName, int mana, int level)
        {
            return $"{crystalName}\nM {Mathf.Max(0, mana)}\nLv {Mathf.Max(1, level)}";
        }

        private struct CrystalSpendFloatingTextEntry
        {
            public TextMeshProUGUI Text;
            public Vector2 StartPosition;
            public Vector2 EndPosition;
            public float RemainingLifetime;
            public float TotalLifetime;
        }

    }
}

