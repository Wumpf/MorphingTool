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
        public MarkerSet MarkerSet
        {
            get { return _markerSet; }
        }
        private MarkerSet _markerSet;

        /// <summary>
        /// currently used algorithm type
        /// </summary>
        private Algorithm _currentAlgorithmType;

        /// <summary>
        /// Algorithm that dissolves two warped Images
        /// </summary>
        private CrossDissolver _crossDissolver = new AlphaBlendDissolver();

        /// <summary>
        /// Intern representation of Image Data.
        /// </summary>
        public unsafe class ImageData : IDisposable
        {
            public Color* Data { get; private set; }
            public readonly int Width;
            public readonly int Height;

            public readonly int BufferSize;
            public readonly int Stride;

            public ImageData(int width, int height)
            {
                Stride = width * sizeof(Color);
                BufferSize = Stride * height;

                Data = (Color*)System.Runtime.InteropServices.Marshal.AllocHGlobal(BufferSize);
                Width = width;
                Height = height;
            }

            ~ImageData()
            {
                Dispose();
            }

            public void Dispose()
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal((IntPtr)Data);
                Data = null;
            }

            /// <summary>
            /// Samples the image data withnormalized 0-1 floating point coordinates.
            /// </summary>
            /// <returns>Sampled Color</returns>
            public Color SampleLinear(float x, float y)
            {
                System.Diagnostics.Debug.Assert(x >= 0.0f && y >= 0.0f && x <= 1.0f && y <= 1.0f);
                int coordFloorX = (int)(x * Width);
                int coordFloorY = (int)(y * Height);
                return Data[coordFloorX + coordFloorY * Width];
            } 
        };

        /// <summary>
        /// Start image in an intern intermediate representation. Every int value is a 32bit color.
        /// </summary>
        private ImageData _startImage;
        /// <summary>
        /// End image in an intern intermediate representation. Every int value is a 32bit color.
        /// </summary>
        private ImageData _endImage;

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
        public void SetStartImage(BitmapSource inputStartImage)
        {
            if(_startImage != null)
                _startImage.Dispose();

            _startImage = new ImageData(inputStartImage.PixelWidth, inputStartImage.PixelHeight);
            unsafe
            {
                inputStartImage.CopyPixels(System.Windows.Int32Rect.Empty, (IntPtr)_startImage.Data, _startImage.BufferSize, _startImage.Stride);
            }
        }

        /// <summary>
        /// Sets the StartImage and copies it into an intern intermediate buffer for faster access.
        /// </summary>
        /// <param name="endImage">Image for morphingProgress=1</param>
        public void SetEndImage(BitmapSource inputEndImage)
        {
            if (_endImage != null)
                _endImage.Dispose();

            _endImage = new ImageData(inputEndImage.PixelWidth, inputEndImage.PixelHeight);
            unsafe
            {
                inputEndImage.CopyPixels(System.Windows.Int32Rect.Empty, (IntPtr)_endImage.Data, _endImage.BufferSize, _endImage.Stride);
            }
        }

        /// <summary>
        /// Performs morphing between two images using the given algorithm and feature-markers.
        /// </summary>
        /// <param name="morphingProgress">Morph-percentage from 0 to 1</param>
        /// <param name="outputImage">target for image output data</param>
        public void MorphImages(float morphingProgress, WriteableBitmap outputImage)
        {
            System.Diagnostics.Debug.Assert(morphingProgress >= 0.0f && morphingProgress <= 1.0f);
            System.Diagnostics.Debug.Assert(_startImage != null && _endImage != null && outputImage != null);

            _markerSet.UpdateInterpolation(morphingProgress);

            _crossDissolver.DissolveImages(_startImage, _endImage, morphingProgress, outputImage);
        }
    }
}
