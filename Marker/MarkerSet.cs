using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace MorphingTool
{
    /// <summary>
    /// Abstract description for feature markers in start and end image.
    /// Every MarkerSet also keeps it current set of interpolated markers.
    /// </summary>
    public abstract class MarkerSet
    {
        /// <summary>
        /// Last used interpolation factor. Will be kept to determine the interpolated position of new Markers.
        /// </summary>
        protected float lastInterpolationFactor;

        public enum MouseLocation
        {
            START_IMAGE = 0,
            END_IMAGE = 1,
            NONE = -1
        }

        public abstract void OnLeftMouseButtonDown(MouseLocation clickLocation, Vector imageCor, Vector imageSizePixel);
        public abstract void OnLeftMouseButtonUp();
        public abstract void OnRightMouseButtonDown(MouseLocation clickLocation, Vector imageCor, Vector imageSizePixel);
        public abstract void OnMouseMove(MouseLocation clickLocation, Vector imageCor);

        /// <summary>
        /// Updates the coordinates/representation of the interpolated marker
        /// </summary>
        /// <param name="interpolation">Interpolation from 0 (startimage) to 1 (endimage)</param>
        public virtual void UpdateInterpolation(float interpolation)
        {
            lastInterpolationFactor = interpolation;
        }

        public abstract void UpdateMarkerCanvas(Canvas[] imageCanvas, Vector[] imageOffsetPixel, Vector[] imageSizePixel);
    }
}
