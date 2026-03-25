using System;
using UnityEngine;

namespace RainbowTower.EnemySystem
{
    [DisallowMultipleComponent]
    public sealed class EnemyView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private int sortingOrder = -4;

        private Vector3[] waypoints;
        private int nextWaypointIndex;
        private float moveSpeed;
        private bool isInitialized;
        private bool hasEscaped;
        private Action<EnemyView> onReachedExit;

        public void Initialize(Vector3[] pathWaypoints, float speed, Color tint, Vector2 scale, Action<EnemyView> reachedExitCallback)
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
            nextWaypointIndex = 1;
            hasEscaped = false;
            isInitialized = true;

            transform.position = waypoints[0];
            transform.localScale = new Vector3(scale.x, scale.y, 1f);
            spriteRenderer.color = tint;
        }

        private void Awake()
        {
            EnsureVisual();
        }

        private void Update()
        {
            if (!isInitialized || hasEscaped)
            {
                return;
            }

            if (nextWaypointIndex >= waypoints.Length)
            {
                Escape();
                return;
            }

            var targetPosition = waypoints[nextWaypointIndex];
            var nextPosition = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            transform.position = nextPosition;

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

            spriteRenderer.sprite = SpriteFactory.WhiteSprite;
            spriteRenderer.sortingOrder = sortingOrder;
        }

        private void Escape()
        {
            if (hasEscaped)
            {
                return;
            }

            hasEscaped = true;
            onReachedExit?.Invoke(this);
            Destroy(gameObject);
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
