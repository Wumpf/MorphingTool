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
        /// Checks weather a Vector is within a rectangle or not
        /// </summary>
        /// <param name="a"></param>
        /// <param name="rectMin"></param>
        /// <param name="rectMax"></param>
        /// <returns></returns>
        public static bool IsInRectangle(this Vector a, Vector rectMin, Vector rectMax)
        {
            return a.X > rectMin.X && a.X < rectMax.X && a.Y > rectMin.Y && a.Y < rectMax.Y;
        }
    }
}
