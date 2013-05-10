using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MorphingTool
{
    /// <summary>
    /// Since Vectorers to any data structure with managed attributes are prohibited, there is an own simple Color class.
    /// </summary>
    public struct Color
    {
        public Color(byte B, byte G, byte R)
        {
            this.B = B;
            this.G = G;
            this.R = R;
            this.A = 255;
        }

        public byte B;
        public byte G;
        public byte R;
        public byte A;

        /// <summary>
        /// Performs a linear interpolation between two colors.
        /// </summary>
        /// <param name="a">first color</param>
        /// <param name="b">second color</param>
        /// <param name="interp">interpolation factor [0;1]</param>
        /// <returns>Linear interpolation between the two colors</returns>
        public static Color Lerp(Color a, Color b, float interp)
        {
            return new Color((byte)(a.B + (float)(b.B - a.B) * interp),
                             (byte)(a.G + (float)(b.G - a.G) * interp),
                             (byte)(a.R + (float)(b.R - a.R) * interp));
        }
    };
}
