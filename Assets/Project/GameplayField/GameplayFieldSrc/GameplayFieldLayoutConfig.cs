using UnityEngine;

namespace RainbowTower.GameplayField
{
    [CreateAssetMenu(
        fileName = "GameplayFieldLayoutConfig",
        menuName = "RainbowTower/GameplayField/Gameplay Field Layout Config")]
    public sealed class GameplayFieldLayoutConfig : ScriptableObject
    {
        [Header("Reference Resolution")]
        [SerializeField] private Vector2 referenceResolution = new(1080f, 1920f);

        [Header("Static Hud")]
        [SerializeField] private int currentHp = 25;
        [SerializeField] private int maxHp = 30;
        [SerializeField] private int currentWave = 1;
        [SerializeField] private int totalWaves = 10;

        [Header("Field Layout Pixels")]
        [SerializeField] private float fieldTopPixels = 155f;
        [SerializeField] private float fieldHeightPixels = 1080f;
        [SerializeField] private float fieldSideInsetPixels = 4f;
        [SerializeField] private Sprite fieldBackgroundSprite;
        [SerializeField] private Sprite towerSprite;
        [SerializeField] private float roadInsetPixels = 8f;
        [SerializeField] private float roadThicknessPixels = 150f;
        [SerializeField] private float sideRouteInsetPixels = 65f;
        [SerializeField] private float portalCenterOffsetPixels = 145f;
        [SerializeField] private Vector2 portalSizePixels = new(92f, 170f);
        [SerializeField] private Vector2 towerBaseSizePixels = new(190f, 250f);
        [SerializeField] private Vector2 towerCoreSizePixels = new(140f, 200f);
        [SerializeField] private Vector2 towerShadowSizePixels = new(220f, 84f);

        [Header("Palette")]
        [SerializeField] private Color outerGrassColor = new(0.32f, 0.63f, 0.16f, 1f);
        [SerializeField] private Color innerGrassColor = new(0.54f, 0.86f, 0.16f, 1f);
        [SerializeField] private Color pathColor = new(0.81f, 0.17f, 0.11f, 1f);
        [SerializeField] private Color portalColor = new(0.26f, 0.71f, 0.98f, 1f);
        [SerializeField] private Color portalGlowColor = new(0.31f, 0.32f, 0.98f, 0.45f);
        [SerializeField] private Color towerBaseColor = new(0.56f, 0.47f, 0.32f, 1f);
        [SerializeField] private Color towerCoreColor = new(0.9f, 0.88f, 0.8f, 1f);
        [SerializeField] private Color shadowColor = new(0f, 0f, 0f, 0.18f);

        public Vector2 ReferenceResolution => referenceResolution;
        public int CurrentHp => currentHp;
        public int MaxHp => maxHp;
        public int CurrentWave => currentWave;
        public int TotalWaves => totalWaves;
        public float FieldTopPixels => fieldTopPixels;
        public float FieldHeightPixels => fieldHeightPixels;
        public float FieldSideInsetPixels => fieldSideInsetPixels;
        public Sprite FieldBackgroundSprite => fieldBackgroundSprite;
        public Sprite TowerSprite => towerSprite;
        public float RoadInsetPixels => roadInsetPixels;
        public float RoadThicknessPixels => roadThicknessPixels;
        public float SideRouteInsetPixels => sideRouteInsetPixels;
        public float PortalCenterOffsetPixels => portalCenterOffsetPixels;
        public Vector2 PortalSizePixels => portalSizePixels;
        public Vector2 TowerBaseSizePixels => towerBaseSizePixels;
        public Vector2 TowerCoreSizePixels => towerCoreSizePixels;
        public Vector2 TowerShadowSizePixels => towerShadowSizePixels;
        public Color OuterGrassColor => outerGrassColor;
        public Color InnerGrassColor => innerGrassColor;
        public Color PathColor => pathColor;
        public Color PortalColor => portalColor;
        public Color PortalGlowColor => portalGlowColor;
        public Color TowerBaseColor => towerBaseColor;
        public Color TowerCoreColor => towerCoreColor;
        public Color ShadowColor => shadowColor;
    }
}


