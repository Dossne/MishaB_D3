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
        Cyan = 5,
        White = 6
    }

    public static class ManaColorUtility
    {
        public const int BaseColorCount = 3;
        public const int TotalColorCount = 7;

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

        public static readonly ManaColor[] NonWhiteColors =
        {
            ManaColor.Red,
            ManaColor.Green,
            ManaColor.Blue,
            ManaColor.Yellow,
            ManaColor.Magenta,
            ManaColor.Cyan
        };

        public static readonly ManaColor[] ConversionColors =
        {
            ManaColor.Yellow,
            ManaColor.Magenta,
            ManaColor.Cyan,
            ManaColor.White
        };

        public static readonly ManaColor[] AllColors =
        {
            ManaColor.Red,
            ManaColor.Green,
            ManaColor.Blue,
            ManaColor.Yellow,
            ManaColor.Magenta,
            ManaColor.Cyan,
            ManaColor.White
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
                ManaColor.White => 6,
                _ => throw new ArgumentOutOfRangeException(nameof(color), color, "Unsupported mana color.")
            };
        }
    }
}
