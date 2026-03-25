using RainbowTower.ManaSystem;
using UnityEngine;

namespace RainbowTower.TapDrops
{
    [CreateAssetMenu(
        fileName = "TapDropConfig",
        menuName = "RainbowTower/TapDrops/Tap Drop Config")]
    public sealed class TapDropConfig : ScriptableObject
    {
        [Header("Feature")]
        [SerializeField] private bool isEnabled = true;
        [SerializeField, Range(0.1f, 0.25f)] private float spawnChancePerShot = 0.15f;
        [SerializeField, Min(1)] private int maxSimultaneousDrops = 3;
        [SerializeField, Min(0.5f)] private float lifetimeSeconds = 3f;

        [Header("Placement")]
        [SerializeField, Min(0f)] private float minSpawnRadiusWorld = 0.2f;
        [SerializeField, Min(0.01f)] private float maxSpawnRadiusWorld = 0.85f;
        [SerializeField, Min(0.05f)] private float tapRadiusWorld = 0.45f;

        [Header("View")]
        [SerializeField] private TapDropView dropPrefab;
        [SerializeField, Min(0.1f)] private float dropWorldSize = 0.3f;
        [SerializeField] private int sortingOrder = -3;

        [Header("Feedback")]
        [SerializeField, Min(0.03f)] private float spawnFeedbackDuration = 0.14f;
        [SerializeField, Min(0f)] private float pulseScaleAmplitude = 0.1f;
        [SerializeField, Min(0f)] private float pulseFrequency = 6f;
        [SerializeField, Min(0.03f)] private float collectFeedbackDuration = 0.18f;
        [SerializeField, Min(0.03f)] private float expireFeedbackDuration = 0.2f;

        public bool IsEnabled => isEnabled;
        public float SpawnChancePerShot => Mathf.Clamp(spawnChancePerShot, 0f, 1f);
        public int MaxSimultaneousDrops => Mathf.Max(1, maxSimultaneousDrops);
        public float LifetimeSeconds => Mathf.Max(0.5f, lifetimeSeconds);
        public float MinSpawnRadiusWorld => Mathf.Max(0f, minSpawnRadiusWorld);
        public float MaxSpawnRadiusWorld => Mathf.Max(MinSpawnRadiusWorld + 0.01f, maxSpawnRadiusWorld);
        public float TapRadiusWorld => Mathf.Max(0.05f, tapRadiusWorld);
        public TapDropView DropPrefab => dropPrefab;
        public float DropWorldSize => Mathf.Max(0.1f, dropWorldSize);
        public int SortingOrder => sortingOrder;
        public float SpawnFeedbackDuration => Mathf.Max(0.03f, spawnFeedbackDuration);
        public float PulseScaleAmplitude => Mathf.Max(0f, pulseScaleAmplitude);
        public float PulseFrequency => Mathf.Max(0f, pulseFrequency);
        public float CollectFeedbackDuration => Mathf.Max(0.03f, collectFeedbackDuration);
        public float ExpireFeedbackDuration => Mathf.Max(0.03f, expireFeedbackDuration);

        public static Color GetColor(ManaColor color)
        {
            return color switch
            {
                ManaColor.Red => new Color(1f, 0.28f, 0.28f, 0.95f),
                ManaColor.Green => new Color(0.33f, 1f, 0.47f, 0.95f),
                ManaColor.Blue => new Color(0.33f, 0.66f, 1f, 0.95f),
                ManaColor.Yellow => new Color(1f, 0.9f, 0.2f, 0.95f),
                ManaColor.Magenta => new Color(1f, 0.35f, 0.86f, 0.95f),
                ManaColor.Cyan => new Color(0.3f, 0.95f, 0.95f, 0.95f),
                ManaColor.White => new Color(1f, 0.98f, 0.9f, 0.95f),
                _ => Color.white
            };
        }
    }
}
