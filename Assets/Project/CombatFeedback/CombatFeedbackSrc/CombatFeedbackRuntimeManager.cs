using System.Collections.Generic;
using RainbowTower.Bootstrap;
using RainbowTower.GameplayField;
using RainbowTower.ManaSystem;
using TMPro;
using UnityEngine;

namespace RainbowTower.CombatFeedback
{
    public sealed class CombatFeedbackRuntimeManager : IRuntimeManager
    {
        private readonly List<SpriteFeedback> activeSprites = new();
        private readonly List<FloatingTextFeedback> activeTexts = new();

        private CombatFeedbackConfig feedbackConfig;
        private Transform towerAnchor;
        private Transform worldFeedbackRoot;
        private AudioSource audioSource;
        private bool isReady;
        private float insufficientManaNextTime;

        public void Initialize(ServiceLocator serviceLocator)
        {
            feedbackConfig = serviceLocator.ConfigurationProvider.GetConfiguration<CombatFeedbackConfig>();
            if (feedbackConfig == null)
            {
                Debug.LogError("CombatFeedbackRuntimeManager requires CombatFeedbackConfig.");
                isReady = false;
                return;
            }

            if (!serviceLocator.TryGet<GameplayFieldProvider>(out var gameplayFieldProvider) || gameplayFieldProvider == null)
            {
                Debug.LogError("CombatFeedbackRuntimeManager requires GameplayFieldProvider.");
                isReady = false;
                return;
            }

            towerAnchor = gameplayFieldProvider.TowerAnchor;
            var rootObject = new GameObject("RuntimeCombatFeedback", typeof(AudioSource));
            worldFeedbackRoot = rootObject.transform;
            worldFeedbackRoot.SetParent(gameplayFieldProvider.FieldVisualRoot, false);

            audioSource = rootObject.GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;

            insufficientManaNextTime = 0f;
            isReady = true;
        }

        public void Tick(float deltaTime)
        {
            if (!isReady)
            {
                return;
            }

            TickSprites(deltaTime);
            TickTexts(deltaTime);
        }

        public void LateTick(float deltaTime)
        {
        }

        public void Deinitialize()
        {
            for (var index = 0; index < activeSprites.Count; index++)
            {
                if (activeSprites[index].Renderer != null)
                {
                    Object.Destroy(activeSprites[index].Renderer.gameObject);
                }
            }

            for (var index = 0; index < activeTexts.Count; index++)
            {
                if (activeTexts[index].Text != null)
                {
                    Object.Destroy(activeTexts[index].Text.gameObject);
                }
            }

            activeSprites.Clear();
            activeTexts.Clear();

            if (worldFeedbackRoot != null)
            {
                Object.Destroy(worldFeedbackRoot.gameObject);
                worldFeedbackRoot = null;
            }

            towerAnchor = null;
            audioSource = null;
            feedbackConfig = null;
            insufficientManaNextTime = 0f;
            isReady = false;
        }

        public void NotifyTowerShot(ManaColor color, Vector3 fromPosition, Vector3 toPosition)
        {
            if (!isReady)
            {
                return;
            }

            CreateBeamFeedback(fromPosition, toPosition, GetColor(color), feedbackConfig.ShotVisualLifetime);
            PlayClip(feedbackConfig.ShotAudioClip);
        }

        public void NotifyEnemyHit(Vector3 worldPosition, ManaColor color, int damage, bool isFatalHit)
        {
            if (!isReady)
            {
                return;
            }

            var hitColor = GetColor(color);
            CreatePulseFeedback(worldPosition, hitColor, feedbackConfig.HitMarkerSize, feedbackConfig.HitVisualLifetime);
            CreateFloatingText($"-{Mathf.Max(1, damage)}", worldPosition + new Vector3(0f, 0.55f, 0f), hitColor, 1.5f);

            if (!isFatalHit)
            {
                PlayClip(feedbackConfig.HitAudioClip);
            }
        }

        public void NotifyEnemyDeath(Vector3 worldPosition, int rewardXp)
        {
            if (!isReady)
            {
                return;
            }

            var deathColor = new Color(1f, 0.93f, 0.75f, 0.95f);
            CreatePulseFeedback(worldPosition, deathColor, feedbackConfig.DeathMarkerSize, feedbackConfig.DeathVisualLifetime);

            if (rewardXp > 0)
            {
                var xpAnchor = towerAnchor != null ? towerAnchor.position : worldPosition;
                CreateFloatingText($"+{rewardXp} XP", xpAnchor + new Vector3(0f, 1.15f, 0f), new Color(1f, 1f, 1f, 1f), 1.5f);
            }

            PlayClip(feedbackConfig.DeathAudioClip);
        }

        public void NotifyInsufficientMana()
        {
            if (!isReady || Time.time < insufficientManaNextTime)
            {
                return;
            }

            insufficientManaNextTime = Time.time + feedbackConfig.WarningFeedbackCooldown;
            var anchorPosition = towerAnchor != null ? towerAnchor.position : Vector3.zero;
            CreateFloatingText("No Mana", anchorPosition + new Vector3(0f, 1.1f, 0f), new Color(1f, 0.67f, 0.24f, 1f));
            PlayClip(feedbackConfig.WarningAudioClip);
        }

        public void NotifyGenerationBlocked(ManaColor blockedColor)
        {
        }

        private void TickSprites(float deltaTime)
        {
            for (var index = activeSprites.Count - 1; index >= 0; index--)
            {
                var entry = activeSprites[index];
                entry.RemainingLifetime -= deltaTime;

                if (entry.RemainingLifetime <= 0f || entry.Renderer == null)
                {
                    if (entry.Renderer != null)
                    {
                        Object.Destroy(entry.Renderer.gameObject);
                    }

                    activeSprites.RemoveAt(index);
                    continue;
                }

                var progress = 1f - entry.RemainingLifetime / entry.TotalLifetime;
                var color = entry.BaseColor;
                color.a = Mathf.Lerp(entry.BaseColor.a, 0f, progress);
                entry.Renderer.color = color;
                entry.Renderer.transform.localScale = Vector3.Lerp(entry.StartScale, entry.EndScale, progress);
                activeSprites[index] = entry;
            }
        }

        private void TickTexts(float deltaTime)
        {
            for (var index = activeTexts.Count - 1; index >= 0; index--)
            {
                var entry = activeTexts[index];
                entry.RemainingLifetime -= deltaTime;

                if (entry.RemainingLifetime <= 0f || entry.Text == null)
                {
                    if (entry.Text != null)
                    {
                        Object.Destroy(entry.Text.gameObject);
                    }

                    activeTexts.RemoveAt(index);
                    continue;
                }

                var progress = 1f - entry.RemainingLifetime / entry.TotalLifetime;
                entry.Text.transform.position = Vector3.Lerp(entry.StartPosition, entry.EndPosition, progress);

                var color = entry.BaseColor;
                var alphaFactor = 1f - Mathf.Pow(progress, 8f);
                color.a = entry.BaseColor.a * Mathf.Clamp01(alphaFactor);
                entry.Text.color = color;
                activeTexts[index] = entry;
            }
        }

        private void CreateBeamFeedback(Vector3 fromPosition, Vector3 toPosition, Color color, float lifetime)
        {
            var direction = toPosition - fromPosition;
            var distance = direction.magnitude;
            if (distance <= 0.01f)
            {
                return;
            }

            var midpoint = (fromPosition + toPosition) * 0.5f;
            var beamRenderer = CreateSpriteObject("ShotBeamFx", midpoint, color, feedbackConfig.WorldFeedbackSortingOrder);
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            beamRenderer.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            var startScale = GetScaleForSize(beamRenderer.sprite, new Vector2(distance, feedbackConfig.ShotBeamThickness));
            var endScale = GetScaleForSize(beamRenderer.sprite, new Vector2(distance, feedbackConfig.ShotBeamThickness * 1.28f));
            beamRenderer.transform.localScale = startScale;

            activeSprites.Add(new SpriteFeedback
            {
                Renderer = beamRenderer,
                BaseColor = color,
                TotalLifetime = Mathf.Max(0.03f, lifetime),
                RemainingLifetime = Mathf.Max(0.03f, lifetime),
                StartScale = startScale,
                EndScale = endScale
            });
        }

        private void CreatePulseFeedback(Vector3 position, Color color, float size, float lifetime)
        {
            var pulseRenderer = CreateSpriteObject("HitPulseFx", position, color, feedbackConfig.WorldFeedbackSortingOrder + 1);
            var startSize = Mathf.Max(0.1f, size * 0.5f);
            var endSize = Mathf.Max(startSize, size * 1.55f);
            var startScale = GetScaleForSize(pulseRenderer.sprite, new Vector2(startSize, startSize));
            var endScale = GetScaleForSize(pulseRenderer.sprite, new Vector2(endSize, endSize));
            pulseRenderer.transform.localScale = startScale;

            activeSprites.Add(new SpriteFeedback
            {
                Renderer = pulseRenderer,
                BaseColor = color,
                TotalLifetime = Mathf.Max(0.03f, lifetime),
                RemainingLifetime = Mathf.Max(0.03f, lifetime),
                StartScale = startScale,
                EndScale = endScale
            });
        }

        private void CreateFloatingText(string text, Vector3 worldPosition, Color color, float sizeMultiplier = 1f)
        {
            var textObject = new GameObject("CombatFloatingText");
            textObject.transform.SetParent(worldFeedbackRoot, false);
            textObject.transform.position = worldPosition;

            var textComponent = textObject.AddComponent<TextMeshPro>();
            textComponent.text = text;
            textComponent.alignment = TextAlignmentOptions.Center;
            textComponent.fontSize = feedbackConfig.FloatingTextSize * Mathf.Max(0.25f, sizeMultiplier);
            textComponent.sortingOrder = feedbackConfig.WorldFeedbackSortingOrder + 2;
            textComponent.color = color;
            textComponent.outlineColor = Color.black;
            textComponent.outlineWidth = 0.15f;
            textComponent.raycastTarget = false;

            if (feedbackConfig.FloatingTextFont != null)
            {
                textComponent.font = feedbackConfig.FloatingTextFont;
            }
            else if (TMP_Settings.defaultFontAsset != null)
            {
                textComponent.font = TMP_Settings.defaultFontAsset;
            }

            var lifetime = feedbackConfig.FloatingTextLifetime;
            activeTexts.Add(new FloatingTextFeedback
            {
                Text = textComponent,
                BaseColor = color,
                TotalLifetime = lifetime,
                RemainingLifetime = lifetime,
                StartPosition = worldPosition,
                EndPosition = worldPosition + new Vector3(0f, feedbackConfig.FloatingTextRiseDistance, 0f)
            });
        }

        private void PlayClip(AudioClip clip)
        {
            if (audioSource == null || clip == null)
            {
                return;
            }

            audioSource.PlayOneShot(clip, feedbackConfig.SfxVolume);
        }

        private SpriteRenderer CreateSpriteObject(string objectName, Vector3 position, Color color, int sortingOrder)
        {
            var spriteObject = new GameObject(objectName, typeof(SpriteRenderer));
            spriteObject.transform.SetParent(worldFeedbackRoot, false);
            spriteObject.transform.position = position;

            var renderer = spriteObject.GetComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.WhiteSprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private static Vector3 GetScaleForSize(Sprite sprite, Vector2 size)
        {
            var bounds = sprite != null ? sprite.bounds.size : Vector3.one;
            var safeWidth = Mathf.Max(0.001f, bounds.x);
            var safeHeight = Mathf.Max(0.001f, bounds.y);
            return new Vector3(size.x / safeWidth, size.y / safeHeight, 1f);
        }

        private static Color GetColor(ManaColor color)
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
                _ => new Color(1f, 1f, 1f, 0.95f)
            };
        }

        private struct SpriteFeedback
        {
            public SpriteRenderer Renderer;
            public Color BaseColor;
            public float TotalLifetime;
            public float RemainingLifetime;
            public Vector3 StartScale;
            public Vector3 EndScale;
        }

        private struct FloatingTextFeedback
        {
            public TextMeshPro Text;
            public Color BaseColor;
            public float TotalLifetime;
            public float RemainingLifetime;
            public Vector3 StartPosition;
            public Vector3 EndPosition;
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
                        whiteSprite.name = "CombatFeedbackWhiteSprite";
                    }

                    return whiteSprite;
                }
            }
        }
    }
}



