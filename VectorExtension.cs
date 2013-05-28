using System;
using System.Windows;

namespace MorphingTool
{
    /// <summary>
    /// A few useful extensions to the buildt-in Vector class.
    /// </summary>
    public static class VectorExtension
    {
        /// <summary>
        /// Performs a linear interpolation between two image positions.
        /// </summary>
        /// <param name="a">first position</param>
        /// <param name="b">second position</param>
        /// <param name="interp">interpolation factor [0;1]</param>
        /// <returns>Linear interpolation between the two positions</returns>
        public static Vector Lerp(this Vector a, Vector b, float interp)
        {
            return new Vector()
            {
                X = a.X + (b.X - a.X) * interp,
                Y = a.Y + (b.Y - a.Y) * interp
            };
        }

        /// <summary>
        /// Performs a dot product between two vectors.
        /// </summary>
        /// <param name="a">first vector</param>
        /// <param name="b">second vector</param>
        /// <returns>Dot Product of a and b</returns>
        public static double Dot(this Vector a, Vector b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public static Vector Perpendicular(this Vector a)
        {
            return new Vector(a.Y, -a.X);
        }


        /// <summary>
        /// Checks weather a Point is within a rectangle or not
        /// </summary>
        /// <param name="a">The Point</param>
        /// <param name="rectMin">Minimum point of the rectangle</param>
        /// <param name="rectMax">Maximum point of the rectangle</param>
        /// <returns>true if within the rectangle, else otherwise</returns>
        public static bool IsInRectangle(this Vector a, Vector rectMin, Vector rectMax)
        {
            return a.X > rectMin.X && a.X < rectMax.X && a.Y > rectMin.Y && a.Y < rectMax.Y;
        }

        /// <summary>
        /// Clamps a vector to the valid image area (0-1) on every axis
        /// </summary>
        /// <param name="v">Vector to clamp</param>
        /// <returns>Clamped vector</returns>
        public static Vector ClampToImageArea(this Vector v)
        {
            return new Vector(v.X < 0 ? 0 : (v.X > 1 ? 1 : v.X),
                              v.Y < 0 ? 0 : (v.Y > 1 ? 1 : v.Y));
        }
    }
}
