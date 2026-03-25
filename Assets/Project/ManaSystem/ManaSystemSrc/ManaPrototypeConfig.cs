using UnityEngine;

namespace RainbowTower.ManaSystem
{
    [CreateAssetMenu(
        fileName = "ManaPrototypeConfig",
        menuName = "RainbowTower/ManaSystem/Mana Prototype Config")]
    public sealed class ManaPrototypeConfig : ScriptableObject
    {
        [SerializeField] private int startRedMana;
        [SerializeField] private int startGreenMana;
        [SerializeField] private int startBlueMana;

        public int GetStartingMana(ManaColor color)
        {
            return color switch
            {
                ManaColor.Red => Mathf.Max(0, startRedMana),
                ManaColor.Green => Mathf.Max(0, startGreenMana),
                ManaColor.Blue => Mathf.Max(0, startBlueMana),
                _ => 0
            };
        }
    }
}

