using System;
using System.Windows.Media;

namespace ColorQuantizer
{
    public class ColorRgb
    {
        public int Red { get; set; } = 0;
        public int Green { get; set; } = 0;
        public int Blue { get; set; } = 0;

        public ColorRgb(int r, int g, int b)
        {
            Red = Math.Min(Math.Max(r,0), 255);
            Green = Math.Min(Math.Max(g, 0), 255);
            Blue = Math.Min(Math.Max(b, 0), 255);
        }

        public Color GetColor()
        {
            return new Color
            {
                R = (byte) Red,
                G = (byte) Green,
                B = (byte) Blue,
                A = 255
            };
        }

        public void AddColor(ColorRgb color)
        {
            Red += color.Red;
            Green += color.Green;
            Blue += color.Blue;
        }
    }
}
