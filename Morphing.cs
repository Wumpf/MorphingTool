using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MorphingTool
{
    /// <summary>
    /// Class performing morphing between two images with a given algorithm.
    /// </summary>
    public class Morphing
    {
        /// <summary>
        /// possible morphing algorithms
        /// </summary>
        public enum Algorithm
        {
            MESH_BASED,
            RADIAL_FUNCTIONS,
            FEATURE_BASED,
        }

        /// <summary>
        /// A set of marker for the current MorphingAlgorithm
        /// </summary>
        private MarkerSet _markerSet;
        private Algorithm _currentAlgorithmType;

        /// <summary>
        /// Algorithm that dissolves two warped Images
        /// </summary>
        private CrossDissolver _crossDissolver = new AlphaBlendDissolver();

        /// <summary>
        /// Active type of algorithm used for morphing.
        /// Changing will reset all marker settings
        /// </summary>
        public Algorithm AlgorithmType
        {
            get { return _currentAlgorithmType; }
            set
            {
                if (value != _currentAlgorithmType)
                {
                    switch (value)
                    {
                        case Algorithm.RADIAL_FUNCTIONS:
                            _markerSet = new PointMarkerSet();
                            break;
                    }
                    _currentAlgorithmType = value;
                }
                
            }
        }

        /// <summary>
        /// Initializes an instance for morphing images
        /// </summary>
        /// <param name="algorithmType">startup algorithm type</param>
        public Morphing(Algorithm algorithmType = Algorithm.RADIAL_FUNCTIONS)
        {
            AlgorithmType = algorithmType;
        }

        /// <summary>
        /// Performs morphing between two images using the given algorithm and feature-markers.
        /// </summary>
        /// <param name="startImage">Image for morphingProgress=0</param>
        /// <param name="endImage">Image for morphingProgress=1</param>
        /// <param name="morphingProgress">Morph-percentage from 0 to 1</param>
        /// <param name="outputImage">target for image output data</param>
        public void MorphImages(ImageSource startImage, ImageSource endImage, float morphingProgress, WriteableBitmap outputImage)
        {
            System.Diagnostics.Debug.Assert(morphingProgress >= 0.0f && morphingProgress <= 1.0f);
            System.Diagnostics.Debug.Assert(startImage != null && endImage != null && outputImage != null);

            _crossDissolver.DissolveImages(startImage, endImage, morphingProgress, outputImage);
        }
    }
}
