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
        private const int TowerShadowOrder = -8;
        private const int TowerOrder = -7;

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
            var innerSize = new Vector2(
                outerSize.x - 2f * (roadInsetWorld + roadThicknessWorld),
                outerSize.y - 2f * (layoutConfig.RoadInsetPixels * pixelsToWorldY + layoutConfig.RoadThicknessPixels * pixelsToWorldY));
            var topLaneY = outerSize.y * 0.5f - layoutConfig.RoadInsetPixels * pixelsToWorldY - layoutConfig.RoadThicknessPixels * pixelsToWorldY * 0.5f;
            var bottomLaneY = -outerSize.y * 0.5f + layoutConfig.RoadInsetPixels * pixelsToWorldY + layoutConfig.RoadThicknessPixels * pixelsToWorldY * 0.5f;
            var leftLaneX = -outerSize.x * 0.5f + roadInsetWorld + roadThicknessWorld * 0.5f;
            var rightLaneX = outerSize.x * 0.5f - roadInsetWorld - roadThicknessWorld * 0.5f;

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

            CreateOrUpdateSprite("OuterGrass", fieldVisualRoot, Vector3.zero, outerSize, layoutConfig.OuterGrassColor, BackgroundOrder);
            CreateOrUpdateSprite("InnerGrass", fieldVisualRoot, Vector3.zero, innerSize, layoutConfig.InnerGrassColor, BackgroundOrder + 1);
            CreateOrUpdateSprite("PathLeft", fieldVisualRoot, new Vector3(leftLaneX, 0f, 0f), new Vector2(roadThicknessWorld, outerSize.y - 2f * layoutConfig.RoadInsetPixels * pixelsToWorldY), layoutConfig.PathColor, PathOrder);
            CreateOrUpdateSprite("PathRight", fieldVisualRoot, new Vector3(rightLaneX, 0f, 0f), new Vector2(roadThicknessWorld, outerSize.y - 2f * layoutConfig.RoadInsetPixels * pixelsToWorldY), layoutConfig.PathColor, PathOrder);
            CreateOrUpdateSprite("PathBottom", fieldVisualRoot, new Vector3(0f, bottomLaneY, 0f), new Vector2(outerSize.x - 2f * roadInsetWorld, layoutConfig.RoadThicknessPixels * pixelsToWorldY), layoutConfig.PathColor, PathOrder);

            var topLeftWidth = Mathf.Max(0.05f, (entranceLocal.x - portalSize.x * 0.5f) - (-outerSize.x * 0.5f + roadInsetWorld));
            CreateOrUpdateSprite(
                "PathTopLeft",
                fieldVisualRoot,
                new Vector3(-outerSize.x * 0.5f + roadInsetWorld + topLeftWidth * 0.5f, topLaneY, 0f),
                new Vector2(topLeftWidth, layoutConfig.RoadThicknessPixels * pixelsToWorldY),
                layoutConfig.PathColor,
                PathOrder);

            var topRightWidth = Mathf.Max(0.05f, (outerSize.x * 0.5f - roadInsetWorld) - (exitLocal.x + portalSize.x * 0.5f));
            CreateOrUpdateSprite(
                "PathTopRight",
                fieldVisualRoot,
                new Vector3(exitLocal.x + portalSize.x * 0.5f + topRightWidth * 0.5f, topLaneY, 0f),
                new Vector2(topRightWidth, layoutConfig.RoadThicknessPixels * pixelsToWorldY),
                layoutConfig.PathColor,
                PathOrder);

            CreatePortal("EntrancePortalGlow", "EntrancePortal", entranceLocal, portalSize, layoutConfig, pixelsToWorldX, pixelsToWorldY);
            CreatePortal("ExitPortalGlow", "ExitPortal", exitLocal, portalSize, layoutConfig, pixelsToWorldX, pixelsToWorldY);

            var towerBaseSize = new Vector2(layoutConfig.TowerBaseSizePixels.x * pixelsToWorldX, layoutConfig.TowerBaseSizePixels.y * pixelsToWorldY);
            var towerCoreSize = new Vector2(layoutConfig.TowerCoreSizePixels.x * pixelsToWorldX, layoutConfig.TowerCoreSizePixels.y * pixelsToWorldY);
            var shadowSize = new Vector2(layoutConfig.TowerShadowSizePixels.x * pixelsToWorldX, layoutConfig.TowerShadowSizePixels.y * pixelsToWorldY);

            CreateOrUpdateSprite("TowerShadow", fieldVisualRoot, new Vector3(0f, -towerBaseSize.y * 0.42f, 0f), shadowSize, layoutConfig.ShadowColor, TowerShadowOrder);
            CreateOrUpdateSprite("TowerBase", fieldVisualRoot, Vector3.zero, towerBaseSize, layoutConfig.TowerBaseColor, TowerOrder);
            CreateOrUpdateSprite("TowerCore", fieldVisualRoot, new Vector3(0f, 0.04f, 0f), towerCoreSize, layoutConfig.TowerCoreColor, TowerOrder + 1);
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
            GameplayFieldLayoutConfig layoutConfig,
            float pixelsToWorldX,
            float pixelsToWorldY)
        {
            CreateOrUpdateSprite(
                glowName,
                fieldVisualRoot,
                localPosition,
                new Vector2(portalSize.x * 1.35f, portalSize.y * 1.15f),
                layoutConfig.PortalGlowColor,
                PortalGlowOrder);
            CreateOrUpdateSprite(portalName, fieldVisualRoot, localPosition, portalSize, layoutConfig.PortalColor, PortalOrder);
        }

        private void CreateOrUpdateSprite(
            string childName,
            Transform parent,
            Vector3 localPosition,
            Vector2 size,
            Color color,
            int sortingOrder)
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
            renderer.sprite = SpriteFactory.WhiteSprite;
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
