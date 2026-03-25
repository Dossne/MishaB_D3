using UnityEngine;

namespace RainbowTower.ProgressionSystem
{
    [CreateAssetMenu(
        fileName = "ProgressionPrototypeConfig",
        menuName = "RainbowTower/ProgressionSystem/Progression Prototype Config")]
    public sealed class ProgressionPrototypeConfig : ScriptableObject
    {
        [SerializeField, Min(0)] private int startXp;

        public int StartXp => Mathf.Max(0, startXp);
    }
}

