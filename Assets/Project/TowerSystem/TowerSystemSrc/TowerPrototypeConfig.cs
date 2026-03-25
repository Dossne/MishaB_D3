using UnityEngine;

namespace RainbowTower.TowerSystem
{
    [CreateAssetMenu(
        fileName = "TowerPrototypeConfig",
        menuName = "RainbowTower/TowerSystem/Tower Prototype Config")]
    public sealed class TowerPrototypeConfig : ScriptableObject
    {
        [SerializeField, Min(0.1f)] private float shotIntervalSeconds = 2f;
        [SerializeField] private Vector2 shotOriginOffset = new(0f, 0.65f);

        public float ShotIntervalSeconds => Mathf.Max(0.1f, shotIntervalSeconds);
        public Vector2 ShotOriginOffset => shotOriginOffset;
    }
}


