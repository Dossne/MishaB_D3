using RainbowTower.Bootstrap;
using UnityEngine;

namespace RainbowTower.GameplayField
{
    [DisallowMultipleComponent]
    public sealed class GameplayFieldProvider : MonoBehaviour
    {
        private const int BackgroundOrder = -20;
        private const int PathOrder = -15;
        private const int PortalGlowOrder = -10;
        private const int PortalOrder = -9;
        private const int TowerOrder = -7;
        private const float FieldBackgroundScale = 0.55f;

        [Header("Scene Anchors")]
        [SerializeField] private Transform fieldVisualRoot;
        [SerializeField] private Transform entrancePortalAnchor;
        [SerializeField] private Transform exitPortalAnchor;
        [SerializeField] private Transform towerAnchor;
        [SerializeField] private GameplayPathDefinition pathDefinition;

        public Transform FieldVisualRoot => fieldVisualRoot;
        public Transform EntrancePortalAnchor => entrancePortalAnchor;
        public Transform ExitPortalAnchor => exitPortalAnchor;
        public Transform TowerAnchor => towerAnchor;
        public GameplayPathDefinition PathDefinition => pathDefinition;

        public void Initialize(ServiceLocator serviceLocator)
        {
            if (serviceLocator == null)
            {
                Debug.LogError("GameplayFieldProvider requires a ServiceLocator instance.", this);
                return;
            }

            if (!HasRequiredSceneReferences())
            {
                Debug.LogError("GameplayFieldProvider is missing required scene references.", this);
                return;
            }

            var layoutConfig = serviceLocator.ConfigurationProvider.GetConfiguration<GameplayFieldLayoutConfig>();
            if (layoutConfig == null)
            {
                Debug.LogError("GameplayFieldLayoutConfig is not assigned in ConfigurationProvider.", this);
                return;
            }

            if (!TryGetCamera(out var worldCamera))
            {
                Debug.LogError("GameplayFieldProvider requires an orthographic camera.", this);
                return;
            }

            BuildVisualShell(layoutConfig, worldCamera);
            serviceLocator.MainUiProvider.SetHudValues(
                layoutConfig.CurrentHp,
                layoutConfig.MaxHp,
                layoutConfig.CurrentWave,
                layoutConfig.TotalWaves);
        }

        private bool HasRequiredSceneReferences()
        {
            return fieldVisualRoot != null
                && entrancePortalAnchor != null
                && exitPortalAnchor != null
                && towerAnchor != null
                && pathDefinition != null
                && pathDefinition.Waypoints != null
                && pathDefinition.WaypointCount >= 7;
        }

        private bool TryGetCamera(out Camera worldCamera)
        {
            worldCamera = Camera.main;
            return worldCamera != null && worldCamera.orthographic;
        }

        private void BuildVisualShell(GameplayFieldLayoutConfig layoutConfig, Camera worldCamera)
        {
            var referenceResolution = layoutConfig.ReferenceResolution;
            var pixelsToWorldX = worldCamera.orthographicSize * 2f * worldCamera.aspect / referenceResolution.x;
            var pixelsToWorldY = worldCamera.orthographicSize * 2f / referenceResolution.y;
            var worldLeft = worldCamera.transform.position.x - worldCamera.orthographicSize * worldCamera.aspect;
            var worldTop = worldCamera.transform.position.y + worldCamera.orthographicSize;

            var outerWidthPixels = referenceResolution.x - layoutConfig.FieldSideInsetPixels * 2f;
            var outerHeightPixels = layoutConfig.FieldHeightPixels;
            var outerSize = new Vector2(outerWidthPixels * pixelsToWorldX, outerHeightPixels * pixelsToWorldY);
            var fieldCenter = new Vector3(
                worldLeft + (layoutConfig.FieldSideInsetPixels + outerWidthPixels * 0.5f) * pixelsToWorldX,
                worldTop - (layoutConfig.FieldTopPixels + outerHeightPixels * 0.5f) * pixelsToWorldY,
                0f);

            fieldVisualRoot.position = fieldCenter;
            fieldVisualRoot.rotation = Quaternion.identity;
            fieldVisualRoot.localScale = Vector3.one;

            var roadInsetWorld = layoutConfig.RoadInsetPixels * pixelsToWorldX;
            var roadThicknessWorld = layoutConfig.RoadThicknessPixels * pixelsToWorldX;
            var topLaneY = outerSize.y * 0.5f - layoutConfig.RoadInsetPixels * pixelsToWorldY - layoutConfig.RoadThicknessPixels * pixelsToWorldY * 0.5f;
            var bottomLaneY = -outerSize.y * 0.5f + layoutConfig.RoadInsetPixels * pixelsToWorldY + layoutConfig.RoadThicknessPixels * pixelsToWorldY * 0.5f;
            var leftLaneX = -outerSize.x * 0.5f + roadInsetWorld + roadThicknessWorld * 0.5f;
            var rightLaneX = outerSize.x * 0.5f - roadInsetWorld - roadThicknessWorld * 0.5f;
            var maxSideRouteInsetWorld = Mathf.Max(0f, (rightLaneX - leftLaneX) * 0.5f - roadThicknessWorld * 0.5f);
            var sideRouteInsetWorld = Mathf.Min(layoutConfig.SideRouteInsetPixels * pixelsToWorldX, maxSideRouteInsetWorld);
            leftLaneX += sideRouteInsetWorld;
            rightLaneX -= sideRouteInsetWorld;

            var portalOffsetX = layoutConfig.PortalCenterOffsetPixels * pixelsToWorldX;
            var portalSize = new Vector2(layoutConfig.PortalSizePixels.x * pixelsToWorldX, layoutConfig.PortalSizePixels.y * pixelsToWorldY);
            var entranceLocal = new Vector3(-portalOffsetX, topLaneY, 0f);
            var exitLocal = new Vector3(portalOffsetX, topLaneY, 0f);

            entrancePortalAnchor.position = fieldVisualRoot.TransformPoint(entranceLocal);
            exitPortalAnchor.position = fieldVisualRoot.TransformPoint(exitLocal);
            towerAnchor.position = fieldCenter;

            ApplyPathWaypointPositions(new[]
            {
                entrancePortalAnchor.position,
                fieldVisualRoot.TransformPoint(new Vector3(leftLaneX, topLaneY, 0f)),
                fieldVisualRoot.TransformPoint(new Vector3(leftLaneX, bottomLaneY, 0f)),
                fieldVisualRoot.TransformPoint(new Vector3(0f, bottomLaneY, 0f)),
                fieldVisualRoot.TransformPoint(new Vector3(rightLaneX, bottomLaneY, 0f)),
                fieldVisualRoot.TransformPoint(new Vector3(rightLaneX, topLaneY, 0f)),
                exitPortalAnchor.position
            });

            CreateOrUpdateSprite(
                "FieldBackground",
                fieldVisualRoot,
                Vector3.zero,
                outerSize,
                Color.white,
                BackgroundOrder,
                layoutConfig.FieldBackgroundSprite != null ? layoutConfig.FieldBackgroundSprite : SpriteFactory.WhiteSprite);
            ForceBackgroundScale();
            RemoveSpriteIfExists("OuterGrass", fieldVisualRoot);
            RemoveSpriteIfExists("InnerGrass", fieldVisualRoot);

            RemoveSpriteIfExists("PathLeft", fieldVisualRoot);
            RemoveSpriteIfExists("PathRight", fieldVisualRoot);
            RemoveSpriteIfExists("PathBottom", fieldVisualRoot);
            RemoveSpriteIfExists("PathTopLeft", fieldVisualRoot);
            RemoveSpriteIfExists("PathTopRight", fieldVisualRoot);

            CreatePortal("EntrancePortalGlow", "EntrancePortal", entranceLocal, portalSize, layoutConfig);
            CreatePortal("ExitPortalGlow", "ExitPortal", exitLocal, portalSize, layoutConfig);

            var towerBaseSize = new Vector2(layoutConfig.TowerBaseSizePixels.x * pixelsToWorldX, layoutConfig.TowerBaseSizePixels.y * pixelsToWorldY);

            RemoveSpriteIfExists("TowerShadow", fieldVisualRoot);
            CreateOrUpdateSprite("TowerVisual", fieldVisualRoot, new Vector3(0f, 0.04f, 0f), towerBaseSize, Color.white, TowerOrder + 1, layoutConfig.TowerSprite != null ? layoutConfig.TowerSprite : SpriteFactory.WhiteSprite);
            RemoveSpriteIfExists("TowerBase", fieldVisualRoot);
            RemoveSpriteIfExists("TowerCore", fieldVisualRoot);
        }
        private void ApplyPathWaypointPositions(Vector3[] positions)
        {
            var waypoints = pathDefinition.Waypoints;
            var count = Mathf.Min(waypoints.Length, positions.Length);

            for (var index = 0; index < count; index++)
            {
                if (waypoints[index] != null)
                {
                    waypoints[index].position = positions[index];
                }
            }
        }

        private void CreatePortal(
            string glowName,
            string portalName,
            Vector3 localPosition,
            Vector2 portalSize,
            GameplayFieldLayoutConfig layoutConfig)
        {
            RemoveSpriteIfExists(glowName, fieldVisualRoot);

            if (layoutConfig.PortalSprite == null)
            {
                Debug.LogError($"Portal sprite is missing in {nameof(GameplayFieldLayoutConfig)}.", this);
                RemoveSpriteIfExists(portalName, fieldVisualRoot);
                return;
            }

            CreateOrUpdateSprite(portalName, fieldVisualRoot, localPosition, portalSize, Color.white, PortalOrder, layoutConfig.PortalSprite);
        }

        private void ForceBackgroundScale()
        {
            var background = fieldVisualRoot != null ? fieldVisualRoot.Find("FieldBackground") : null;
            if (background != null)
            {
                background.localScale = new Vector3(FieldBackgroundScale, FieldBackgroundScale, 1f);
            }
        }
        private static void RemoveSpriteIfExists(string childName, Transform parent)
        {
            var child = parent.Find(childName);
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }

        private void CreateOrUpdateSprite(
            string childName,
            Transform parent,
            Vector3 localPosition,
            Vector2 size,
            Color color,
            int sortingOrder,
            Sprite sprite = null)
        {
            var child = parent.Find(childName);
            if (child == null)
            {
                child = new GameObject(childName, typeof(SpriteRenderer)).transform;
                child.SetParent(parent, false);
            }

            child.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);
            child.localRotation = Quaternion.identity;

            var renderer = child.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite != null ? sprite : SpriteFactory.WhiteSprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;

            var spriteBounds = renderer.sprite.bounds.size;
            var scaleX = spriteBounds.x > Mathf.Epsilon ? size.x / spriteBounds.x : size.x;
            var scaleY = spriteBounds.y > Mathf.Epsilon ? size.y / spriteBounds.y : size.y;
            child.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        private static class SpriteFactory
        {
            private static Sprite whiteSprite;

            public static Sprite WhiteSprite
            {
                get
                {
                    if (whiteSprite == null)
                    {
                        whiteSprite = Sprite.Create(
                            Texture2D.whiteTexture,
                            new Rect(0f, 0f, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
                            new Vector2(0.5f, 0.5f),
                            100f);
                        whiteSprite.name = "GameplayFieldRuntimeWhiteSprite";
                    }

                    return whiteSprite;
                }
            }
        }
    }
}



