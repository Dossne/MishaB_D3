using UnityEngine;

namespace RainbowTower.TowerSystem
{
    [CreateAssetMenu(
        fileName = "TowerPrototypeConfig",
        menuName = "RainbowTower/TowerSystem/Tower Prototype Config")]
    public sealed class TowerPrototypeConfig : ScriptableObject
    {
        [SerializeField, Min(0.1f)] private float shotIntervalSeconds = 2f;

        public float ShotIntervalSeconds => Mathf.Max(0.1f, shotIntervalSeconds);
    }
}

