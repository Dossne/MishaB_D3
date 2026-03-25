using TMPro;
using UnityEngine;

namespace RainbowTower.CombatFeedback
{
    [CreateAssetMenu(
        fileName = "CombatFeedbackConfig",
        menuName = "RainbowTower/CombatFeedback/Combat Feedback Config")]
    public sealed class CombatFeedbackConfig : ScriptableObject
    {
        [Header("Floating Text")]
        [SerializeField] private TMP_FontAsset floatingTextFont;
        [SerializeField, Min(0.2f)] private float floatingTextLifetime = 0.85f;
        [SerializeField, Min(0.1f)] private float floatingTextRiseDistance = 0.75f;
        [SerializeField, Min(0.5f)] private float floatingTextSize = 2.25f;

        [Header("World Visuals")]
        [SerializeField, Min(0.02f)] private float shotBeamThickness = 0.08f;
        [SerializeField, Min(0.03f)] private float shotVisualLifetime = 0.12f;
        [SerializeField, Min(0.03f)] private float hitVisualLifetime = 0.14f;
        [SerializeField, Min(0.03f)] private float deathVisualLifetime = 0.2f;
        [SerializeField, Min(0.1f)] private float hitMarkerSize = 0.25f;
        [SerializeField, Min(0.1f)] private float deathMarkerSize = 0.475f;
        [SerializeField] private int worldFeedbackSortingOrder = -2;

        [Header("Warning Rate Limits")]
        [SerializeField, Min(0.05f)] private float warningFeedbackCooldown = 0.9f;
        [SerializeField, Min(0.1f)] private float blockedGenerationFeedbackCooldown = 1.35f;

        [Header("Audio Hooks")]
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.33f;
        [SerializeField] private AudioClip shotAudioClip;
        [SerializeField] private AudioClip hitAudioClip;
        [SerializeField] private AudioClip deathAudioClip;
        [SerializeField] private AudioClip warningAudioClip;

        public TMP_FontAsset FloatingTextFont => floatingTextFont;
        public float FloatingTextLifetime => Mathf.Max(0.2f, floatingTextLifetime);
        public float FloatingTextRiseDistance => Mathf.Max(0.1f, floatingTextRiseDistance);
        public float FloatingTextSize => Mathf.Max(0.5f, floatingTextSize);
        public float ShotBeamThickness => Mathf.Max(0.02f, shotBeamThickness);
        public float ShotVisualLifetime => Mathf.Max(0.03f, shotVisualLifetime);
        public float HitVisualLifetime => Mathf.Max(0.03f, hitVisualLifetime);
        public float DeathVisualLifetime => Mathf.Max(0.03f, deathVisualLifetime);
        public float HitMarkerSize => Mathf.Max(0.1f, hitMarkerSize);
        public float DeathMarkerSize => Mathf.Max(0.1f, deathMarkerSize);
        public int WorldFeedbackSortingOrder => worldFeedbackSortingOrder;
        public float WarningFeedbackCooldown => Mathf.Max(0.05f, warningFeedbackCooldown);
        public float BlockedGenerationFeedbackCooldown => Mathf.Max(0.1f, blockedGenerationFeedbackCooldown);
        public float SfxVolume => Mathf.Clamp01(sfxVolume);
        public AudioClip ShotAudioClip => shotAudioClip;
        public AudioClip HitAudioClip => hitAudioClip;
        public AudioClip DeathAudioClip => deathAudioClip;
        public AudioClip WarningAudioClip => warningAudioClip;
    }
}



