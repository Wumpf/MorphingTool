using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MorphingTool
{
    /// <summary>
    /// Position on an Image. Normally both axis are from 0 to 1.
    /// </summary>
    public struct ImagePosition
    {
        public ImagePosition(float X, float Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public float X;
        public float Y;


        /// <summary>
        /// Performs a linear interpolation between two image positions.
        /// </summary>
        /// <param name="a">first position</param>
        /// <param name="b">second position</param>
        /// <param name="interp">interpolation factor [0;1]</param>
        /// <returns>Linear interpolation between the two positions</returns>
        public static ImagePosition Lerp(ImagePosition a, ImagePosition b, float interp)
        {
            return new ImagePosition()
            {
                X = a.X + (b.X - a.X) * interp,
                Y = a.Y + (b.Y - a.Y) * interp
            };
        }
    }
}
