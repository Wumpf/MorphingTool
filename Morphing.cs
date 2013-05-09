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
        /// Start image in an intern intermediate representation. Every int value is a 32bit color.
        /// </summary>
        private UInt32[,] _startImage;
        /// <summary>
        /// End image in an intern intermediate representation. Every int value is a 32bit color.
        /// </summary>
        private UInt32[,] _endImage;

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
        /// Sets the StartImage and copies it into an intern intermediate buffer for faster access.
        /// </summary>
        /// <param name="endImage">Image for morphingProgress=1</param>
        public void SetStartImage(BitmapSource startImage)
        {
            _startImage = new UInt32[startImage.PixelWidth, startImage.PixelHeight];
            startImage.CopyPixels(_startImage, startImage.PixelWidth * sizeof(UInt32), 0);
        }

        /// <summary>
        /// Sets the StartImage and copies it into an intern intermediate buffer for faster access.
        /// </summary>
        /// <param name="endImage">Image for morphingProgress=1</param>
        public void SetEndImage(BitmapSource endImage)
        {
            _endImage = new UInt32[endImage.PixelWidth, endImage.PixelHeight];
            endImage.CopyPixels(_endImage, endImage.PixelWidth * sizeof(UInt32), 0);
        }

        /// <summary>
        /// Performs morphing between two images using the given algorithm and feature-markers.
        /// </summary>
        /// <param name="startImage">Image for morphingProgress=0</param>
        
        /// <param name="morphingProgress">Morph-percentage from 0 to 1</param>
        /// <param name="outputImage">target for image output data</param>
        public void MorphImages(float morphingProgress, WriteableBitmap outputImage)
        {
            System.Diagnostics.Debug.Assert(morphingProgress >= 0.0f && morphingProgress <= 1.0f);
            System.Diagnostics.Debug.Assert(_startImage != null && _endImage != null && outputImage != null);

            _crossDissolver.DissolveImages(_startImage, _endImage, morphingProgress, outputImage);
        }
    }
}
