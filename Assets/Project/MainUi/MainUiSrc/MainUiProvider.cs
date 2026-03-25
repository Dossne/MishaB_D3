using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        private const float ShelfTitleHeight = 72f;
        private const float ShelfSpacing = 12f;

        [Header("Font")]
        [SerializeField] private TMP_FontAsset uiFontAsset;

        [Header("Roots")]
        [SerializeField] private RectTransform floatingTextParent;
        [SerializeField] private RectTransform hudParent;
        [SerializeField] private RectTransform popupParent;

        [Header("Hud")]
        [SerializeField] private TMP_Text hpLabel;
        [SerializeField] private TMP_Text waveLabel;

        public RectTransform FloatingTextParent => floatingTextParent;
        public RectTransform HudParent => hudParent;
        public RectTransform PopupParent => popupParent;
        public TMP_Text HpLabel => hpLabel;
        public TMP_Text WaveLabel => waveLabel;

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

        private void Awake()
        {
            EnsureCanvasSetup();
            EnsureHierarchy();
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

            BuildHud();
            BuildCrystalShelf();
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
            if (hudParent.Find("TopHudPanel") != null)
            {
                AssignTopHudReferences();
                return;
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
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = true;

            var wavePanel = CreateLayoutPanel("WavePanel", topHudPanel, new Color(0.95f, 0.68f, 0.12f, 0.95f), 1f, 0f);
            waveLabel = CreateStretchText("WaveLabel", wavePanel, "Wave 1/10", 52f);
            waveLabel.color = new Color(0.98f, 0.97f, 0.88f, 1f);

            var hpPanel = CreateLayoutPanel("HpPanel", topHudPanel, new Color(0.63f, 0.32f, 0.08f, 0.95f), 0f, 280f);
            hpLabel = CreateStretchText("HpLabel", hpPanel, "HP 25/30", 46f);
            hpLabel.color = new Color(1f, 0.95f, 0.84f, 1f);
        }

        private void BuildCrystalShelf()
        {
            if (hudParent.Find("CrystalShelfPanel") != null)
            {
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

            CreateAnchoredText(
                "ShelfTitle",
                shelfPanel,
                "Crystal Shelf",
                38f,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(20f, -(20f + ShelfTitleHeight)),
                new Vector2(-20f, -20f));

            var rowsObject = new GameObject("ShelfRows", typeof(RectTransform));
            var rowsTransform = (RectTransform)rowsObject.transform;
            rowsTransform.SetParent(shelfPanel, false);
            rowsTransform.anchorMin = new Vector2(0f, 0f);
            rowsTransform.anchorMax = new Vector2(1f, 1f);
            rowsTransform.offsetMin = new Vector2(20f, 20f);
            rowsTransform.offsetMax = new Vector2(-20f, -(ShelfTitleHeight + 34f));
            rowsTransform.localScale = Vector3.one;

            var rowsLayout = rowsTransform.gameObject.AddComponent<VerticalLayoutGroup>();
            rowsLayout.padding = new RectOffset(0, 0, 0, 0);
            rowsLayout.spacing = ShelfSpacing;
            rowsLayout.childAlignment = TextAnchor.UpperCenter;
            rowsLayout.childControlWidth = true;
            rowsLayout.childControlHeight = true;
            rowsLayout.childForceExpandWidth = true;
            rowsLayout.childForceExpandHeight = true;

            CreateCrystalRow("TopRow", rowsTransform, "Red", "Green", "Blue");
            CreateCrystalRow("MiddleRow", rowsTransform, "Yellow", "Magenta", "Cyan");
            CreateCrystalRow("BottomRow", rowsTransform, "White");
        }

        private void AssignTopHudReferences()
        {
            hpLabel = FindText(hudParent, "HpLabel");
            waveLabel = FindText(hudParent, "WaveLabel");
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
            var slotObject = new GameObject($"{crystalName}Slot", typeof(RectTransform), typeof(Image));
            var slotTransform = (RectTransform)slotObject.transform;
            slotTransform.SetParent(parent, false);
            slotTransform.localScale = Vector3.one;

            var layoutElement = slotObject.AddComponent<LayoutElement>();
            layoutElement.flexibleHeight = 1f;
            layoutElement.flexibleWidth = 1f;
            layoutElement.minHeight = 0f;
            layoutElement.preferredHeight = 0f;

            var image = slotObject.GetComponent<Image>();
            image.color = Color.Lerp(GetCrystalColor(crystalName), new Color(0.3f, 0.18f, 0.08f, 1f), 0.22f);

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            var labelTransform = (RectTransform)labelObject.transform;
            labelTransform.SetParent(slotTransform, false);
            labelTransform.anchorMin = Vector2.zero;
            labelTransform.anchorMax = Vector2.one;
            labelTransform.offsetMin = new Vector2(10f, 6f);
            labelTransform.offsetMax = new Vector2(-10f, -6f);
            labelTransform.localScale = Vector3.one;

            var label = ConfigureText(labelObject.GetComponent<TextMeshProUGUI>(), crystalName, 30f, FontStyles.Bold);
            label.alignment = TextAlignmentOptions.Center;
            label.color = crystalName == "White" ? new Color(0.18f, 0.12f, 0.22f, 1f) : Color.white;
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

        private Color GetCrystalColor(string crystalName)
        {
            return crystalName switch
            {
                "Red" => new Color(0.86f, 0.22f, 0.26f, 1f),
                "Green" => new Color(0.22f, 0.72f, 0.34f, 1f),
                "Blue" => new Color(0.2f, 0.45f, 0.9f, 1f),
                "Yellow" => new Color(0.92f, 0.78f, 0.2f, 1f),
                "Magenta" => new Color(0.82f, 0.24f, 0.7f, 1f),
                "Cyan" => new Color(0.18f, 0.78f, 0.82f, 1f),
                "White" => new Color(0.95f, 0.94f, 0.88f, 1f),
                _ => new Color(0.3f, 0.3f, 0.3f, 1f)
            };
        }
    }
}
