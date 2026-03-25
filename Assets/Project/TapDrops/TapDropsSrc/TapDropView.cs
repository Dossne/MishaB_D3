using UnityEngine;

using RainbowTower.ManaSystem;

namespace RainbowTower.TapDrops
{
    [DisallowMultipleComponent]
    public sealed class TapDropView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        private VisualState state;
        private Color baseColor;
        private float baseAlpha;
        private Vector3 baseScale;
        private float spawnDuration;
        private float pulseScaleAmplitude;
        private float pulseFrequency;
        private float collectDuration;
        private float expireDuration;
        private float stateTimer;
        private float pulseTimer;

        public void Initialize(
            ManaColor manaColor,
            Color color,
            Sprite dropSprite,
            float worldSize,
            int sortingOrder,
            float spawnFeedbackDuration,
            float pulseAmplitude,
            float pulseSpeed,
            float collectFeedbackDuration,
            float expireFeedbackDuration)
        {
            EnsureRenderer();

            baseColor = dropSprite != null ? new Color(1f, 1f, 1f, color.a) : color;
            baseAlpha = color.a;
            spawnDuration = Mathf.Max(0.03f, spawnFeedbackDuration);
            pulseScaleAmplitude = Mathf.Max(0f, pulseAmplitude);
            pulseFrequency = Mathf.Max(0f, pulseSpeed);
            collectDuration = Mathf.Max(0.03f, collectFeedbackDuration);
            expireDuration = Mathf.Max(0.03f, expireFeedbackDuration);
            state = VisualState.Spawning;
            stateTimer = 0f;
            pulseTimer = 0f;

            spriteRenderer.sprite = ResolveSpriteForColor(manaColor, dropSprite);
            spriteRenderer.sortingOrder = sortingOrder;
            spriteRenderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);

            baseScale = GetScaleForSize(spriteRenderer.sprite, worldSize);
            transform.localScale = Vector3.zero;
        }

        public void TriggerCollect()
        {
            if (state == VisualState.Collected || state == VisualState.Expired)
            {
                return;
            }

            state = VisualState.Collected;
            stateTimer = collectDuration;
        }

        public void TriggerExpire()
        {
            if (state == VisualState.Collected || state == VisualState.Expired)
            {
                return;
            }

            state = VisualState.Expired;
            stateTimer = expireDuration;
        }

        private void Update()
        {
            switch (state)
            {
                case VisualState.Spawning:
                    TickSpawning(Time.deltaTime);
                    break;
                case VisualState.Alive:
                    TickAlive(Time.deltaTime);
                    break;
                case VisualState.Collected:
                    TickCollected(Time.deltaTime);
                    break;
                case VisualState.Expired:
                    TickExpired(Time.deltaTime);
                    break;
            }
        }

        private void TickSpawning(float deltaTime)
        {
            stateTimer += deltaTime;
            var progress = Mathf.Clamp01(stateTimer / spawnDuration);
            var eased = 1f - Mathf.Pow(1f - progress, 3f);

            transform.localScale = Vector3.Lerp(Vector3.zero, baseScale, eased);
            var color = baseColor;
            color.a = baseAlpha * eased;
            spriteRenderer.color = color;

            if (progress >= 1f)
            {
                state = VisualState.Alive;
                stateTimer = 0f;
                transform.localScale = baseScale;
                spriteRenderer.color = baseColor;
            }
        }

        private void TickAlive(float deltaTime)
        {
            pulseTimer += deltaTime * pulseFrequency;
            var pulse = 1f + Mathf.Sin(pulseTimer) * pulseScaleAmplitude;
            transform.localScale = baseScale * pulse;
            spriteRenderer.color = baseColor;
        }

        private void TickCollected(float deltaTime)
        {
            stateTimer -= deltaTime;
            var progress = 1f - Mathf.Clamp01(stateTimer / collectDuration);
            var color = Color.Lerp(baseColor, Color.white, progress);
            color.a = Mathf.Lerp(baseAlpha, 0f, progress);
            spriteRenderer.color = color;

            var scale = 1f + progress * 0.85f;
            transform.localScale = baseScale * scale;

            if (stateTimer <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void TickExpired(float deltaTime)
        {
            stateTimer -= deltaTime;
            var progress = 1f - Mathf.Clamp01(stateTimer / expireDuration);
            var color = Color.Lerp(baseColor, new Color(0.35f, 0.35f, 0.35f, 0.05f), progress);
            color.a = Mathf.Lerp(baseAlpha, 0f, progress);
            spriteRenderer.color = color;

            transform.localScale = Vector3.Lerp(baseScale, Vector3.zero, progress);

            if (stateTimer <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void EnsureRenderer()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }

        private static Sprite ResolveSpriteForColor(ManaColor manaColor, Sprite configuredSprite)
        {
            if (configuredSprite != null)
            {
                return configuredSprite;
            }

            return SpriteFactory.WhiteSprite;
        }

        private static Vector3 GetScaleForSize(Sprite sprite, float worldSize)
        {
            var bounds = sprite != null ? sprite.bounds.size : Vector3.one;
            var safeWidth = Mathf.Max(0.001f, bounds.x);
            var safeHeight = Mathf.Max(0.001f, bounds.y);
            return new Vector3(worldSize / safeWidth, worldSize / safeHeight, 1f);
        }

        private enum VisualState
        {
            Spawning,
            Alive,
            Collected,
            Expired
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
                        whiteSprite.name = "TapDropWhiteSprite";
                    }

                    return whiteSprite;
                }
            }
        }
    }
}

