using System;
using UnityEngine;

namespace RainbowTower.EnemySystem
{
    [DisallowMultipleComponent]
    public sealed class EnemyView : MonoBehaviour
    {
        private const float HitFlashDuration = 0.09f;
        private const float TargetSpriteMaxWorldSize = 2.56f;

        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private int sortingOrder = -4;

        private Vector3[] waypoints;
        private int nextWaypointIndex;
        private float moveSpeed;
        private float totalPathLength;
        private float traversedPathLength;
        private float hitFlashTimer;
        private bool isInitialized;
        private bool isRemoved;
        private Color baseTint;
        private Action<EnemyView> onReachedExit;
        private Action<EnemyView> onKilled;

        public int CurrentHp { get; private set; }
        public int RewardXp { get; private set; }
        public bool IsAlive => isInitialized && !isRemoved && CurrentHp > 0;
        public float ProgressToExit => totalPathLength <= Mathf.Epsilon ? 0f : Mathf.Clamp01(traversedPathLength / totalPathLength);

        public void Initialize(
            Vector3[] pathWaypoints,
            float speed,
            Color tint,
            Vector2 scale,
            int startHp,
            int rewardXp,
            Sprite enemySprite,
            Action<EnemyView> reachedExitCallback,
            Action<EnemyView> killedCallback)
        {
            if (pathWaypoints == null || pathWaypoints.Length < 2)
            {
                Debug.LogError("EnemyView requires at least two path waypoints.", this);
                enabled = false;
                return;
            }

            EnsureVisual();

            waypoints = pathWaypoints;
            moveSpeed = Mathf.Max(0.05f, speed);
            onReachedExit = reachedExitCallback;
            onKilled = killedCallback;
            nextWaypointIndex = 1;
            totalPathLength = CalculatePathLength(pathWaypoints);
            traversedPathLength = 0f;
            CurrentHp = Mathf.Max(1, startHp);
            RewardXp = Mathf.Max(0, rewardXp);
            isInitialized = true;
            isRemoved = false;
            hitFlashTimer = 0f;
            baseTint = tint;
            if (enemySprite != null)
            {
                spriteRenderer.sprite = enemySprite;
            }

            transform.position = waypoints[0];
            var normalizedScale = NormalizeScale(scale, spriteRenderer != null ? spriteRenderer.sprite : null);
            transform.localScale = new Vector3(normalizedScale.x, normalizedScale.y, 1f);
            spriteRenderer.color = baseTint;
        }

        public bool ApplyDamage(int damage)
        {
            if (!IsAlive)
            {
                return false;
            }

            var actualDamage = Mathf.Max(1, damage);
            CurrentHp = Mathf.Max(0, CurrentHp - actualDamage);
            TriggerHitFlash();

            if (CurrentHp > 0)
            {
                return false;
            }

            Die();
            return true;
        }

        private void Awake()
        {
            EnsureVisual();
            baseTint = spriteRenderer != null ? spriteRenderer.color : Color.white;
        }

        private void Update()
        {
            if (!isInitialized || isRemoved)
            {
                return;
            }

            UpdateHitFlash(Time.deltaTime);

            if (nextWaypointIndex >= waypoints.Length)
            {
                Escape();
                return;
            }

            var currentPosition = transform.position;
            var targetPosition = waypoints[nextWaypointIndex];
            var nextPosition = Vector3.MoveTowards(currentPosition, targetPosition, moveSpeed * Time.deltaTime);
            transform.position = nextPosition;

            traversedPathLength += Vector3.Distance(currentPosition, nextPosition);

            if ((targetPosition - nextPosition).sqrMagnitude <= 0.0001f)
            {
                nextWaypointIndex++;
            }
        }

        private void EnsureVisual()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            if (spriteRenderer.sprite == null)
            {
                spriteRenderer.sprite = SpriteFactory.WhiteSprite;
            }
            spriteRenderer.sortingOrder = sortingOrder;
        }

        private void TriggerHitFlash()
        {
            hitFlashTimer = HitFlashDuration;
            spriteRenderer.color = Color.white;
        }

        private void UpdateHitFlash(float deltaTime)
        {
            if (hitFlashTimer <= 0f)
            {
                spriteRenderer.color = baseTint;
                return;
            }

            hitFlashTimer -= deltaTime;
            var normalized = 1f - Mathf.Clamp01(hitFlashTimer / HitFlashDuration);
            spriteRenderer.color = Color.Lerp(Color.white, baseTint, normalized);
        }

        private void Escape()
        {
            if (isRemoved)
            {
                return;
            }

            isRemoved = true;
            traversedPathLength = totalPathLength;
            onReachedExit?.Invoke(this);
            Destroy(gameObject);
        }

        private void Die()
        {
            if (isRemoved)
            {
                return;
            }

            isRemoved = true;
            onKilled?.Invoke(this);
            Destroy(gameObject);
        }

        private static Vector2 NormalizeScale(Vector2 baseScale, Sprite sprite)
        {
            if (sprite == null)
            {
                return baseScale;
            }

            var spriteSize = sprite.bounds.size;
            var maxDimension = Mathf.Max(spriteSize.x, spriteSize.y);
            if (maxDimension <= Mathf.Epsilon)
            {
                return baseScale;
            }

            var factor = TargetSpriteMaxWorldSize / maxDimension;
            return baseScale * factor;
        }

        private static float CalculatePathLength(Vector3[] pathWaypoints)
        {
            var pathLength = 0f;
            for (var index = 1; index < pathWaypoints.Length; index++)
            {
                pathLength += Vector3.Distance(pathWaypoints[index - 1], pathWaypoints[index]);
            }

            return pathLength;
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
                        whiteSprite.name = "EnemyRuntimeWhiteSprite";
                    }

                    return whiteSprite;
                }
            }
        }
    }
}

