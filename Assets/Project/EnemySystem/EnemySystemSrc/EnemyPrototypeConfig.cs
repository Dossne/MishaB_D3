using UnityEngine;

namespace RainbowTower.EnemySystem
{
    [CreateAssetMenu(
        fileName = "EnemyPrototypeConfig",
        menuName = "RainbowTower/EnemySystem/Enemy Prototype Config")]
    public sealed class EnemyPrototypeConfig : ScriptableObject
    {
        [SerializeField] private EnemyView enemyPrefab;
        [SerializeField] private float moveSpeed = 1.35f;
        [SerializeField] private int baseHp = 5;
        [SerializeField] private int baseRewardXp = 1;
        [SerializeField] private Color enemyTint = new(0.22f, 0.74f, 0.29f, 1f);
        [SerializeField] private Vector2 enemyScale = new(6f, 6f);
        [SerializeField] private Sprite[] enemySprites;

        public EnemyView EnemyPrefab => enemyPrefab;
        public float MoveSpeed => moveSpeed;
        public int BaseHp => baseHp;
        public int BaseRewardXp => baseRewardXp;
        public Color EnemyTint => enemyTint;
        public Vector2 EnemyScale => enemyScale;
        public Sprite[] EnemySprites => enemySprites;
    }
}

