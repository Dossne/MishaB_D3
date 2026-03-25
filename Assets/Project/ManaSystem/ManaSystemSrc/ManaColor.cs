using System;

namespace RainbowTower.ManaSystem
{
    public enum ManaColor
    {
        Red = 0,
        Green = 1,
        Blue = 2,
        Yellow = 3,
        Magenta = 4,
        Cyan = 5
    }

    public static class ManaColorUtility
    {
        public const int BaseColorCount = 3;
        public const int Stage6ColorCount = 6;

        public static readonly ManaColor[] BaseColors =
        {
            ManaColor.Red,
            ManaColor.Green,
            ManaColor.Blue
        };

        public static readonly ManaColor[] MixedColors =
        {
            ManaColor.Yellow,
            ManaColor.Magenta,
            ManaColor.Cyan
        };

        public static readonly ManaColor[] Stage6Colors =
        {
            ManaColor.Red,
            ManaColor.Green,
            ManaColor.Blue,
            ManaColor.Yellow,
            ManaColor.Magenta,
            ManaColor.Cyan
        };

        public static int ToIndex(this ManaColor color)
        {
            return color switch
            {
                ManaColor.Red => 0,
                ManaColor.Green => 1,
                ManaColor.Blue => 2,
                ManaColor.Yellow => 3,
                ManaColor.Magenta => 4,
                ManaColor.Cyan => 5,
                _ => throw new ArgumentOutOfRangeException(nameof(color), color, "Unsupported mana color.")
            };
        }
    }
}
