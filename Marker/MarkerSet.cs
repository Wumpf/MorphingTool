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
            START_IMAGE,
            END_IMAGE,
            NONE
        }

        public abstract void OnLeftClick(MouseLocation clickLocation, ImagePosition imageCor);
        public abstract void OnRightClick(MouseLocation clickLocation, ImagePosition imageCor);
        public abstract void OnMouseMove(MouseLocation clickLocation, ImagePosition imageCor);

        /// <summary>
        /// Updates the coordinates/representation of the interpolated marker
        /// </summary>
        /// <param name="interpolation">Interpolation from 0 (startimage) to 1 (endimage)</param>
        public virtual void UpdateInterpolation(float interpolation)
        {
            lastInterpolationFactor = interpolation;
        }

        public abstract void UpdateMarkerCanvas(Canvas[] imageCanvas, Point[] imageOffsetPixel, Point[] imageSizePixel);
    }
}
