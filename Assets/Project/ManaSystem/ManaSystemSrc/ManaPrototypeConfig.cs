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
        [SerializeField] private int startYellowMana;
        [SerializeField] private int startMagentaMana;
        [SerializeField] private int startCyanMana;
        [SerializeField] private int startWhiteMana;

        public int GetStartingMana(ManaColor color)
        {
            return color switch
            {
                ManaColor.Red => Mathf.Max(0, startRedMana),
                ManaColor.Green => Mathf.Max(0, startGreenMana),
                ManaColor.Blue => Mathf.Max(0, startBlueMana),
                ManaColor.Yellow => Mathf.Max(0, startYellowMana),
                ManaColor.Magenta => Mathf.Max(0, startMagentaMana),
                ManaColor.Cyan => Mathf.Max(0, startCyanMana),
                ManaColor.White => Mathf.Max(0, startWhiteMana),
                _ => 0
            };
        }
    }
}
