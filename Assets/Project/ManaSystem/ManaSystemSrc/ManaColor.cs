using System;

namespace RainbowTower.ManaSystem
{
    public enum ManaColor
    {
        Red = 0,
        Green = 1,
        Blue = 2
    }

    public static class ManaColorUtility
    {
        public const int BaseColorCount = 3;

        public static int ToIndex(this ManaColor color)
        {
            return color switch
            {
                ManaColor.Red => 0,
                ManaColor.Green => 1,
                ManaColor.Blue => 2,
                _ => throw new ArgumentOutOfRangeException(nameof(color), color, "Unsupported mana color.")
            };
        }
    }
}

