using Microsoft.Win32;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MorphingTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Morphing _morphingAlgorithm = new Morphing();

        private BitmapSource _originalStartImage;
        private BitmapSource _originalEndImage;

        public MainWindow()
        {
            InitializeComponent();
            _animationPlayer.Tick += AnimationPlayerTimeElapsed;
            RenderVideo.IsEnabled = System.IO.File.Exists("ffmpeg.exe");
        }

        /// <summary>
        /// eventhandler for clicking on the loadstartimage button
        /// </summary>
        private void OnClick_LoadStartImage(object sender, RoutedEventArgs e)
        {
            BitmapImage image = LoadImageFileDialog();
            if (image != null)
            {
                StartImage.Source = _originalStartImage = image;
                AdaptInputOutputImages();
            }
        }

        /// <summary>
        /// eventhandler for clicking on the loadendimage button
        /// </summary>
        private void OnClick_LoadEndImage(object sender, RoutedEventArgs e)
        {
            BitmapImage image = LoadImageFileDialog();
            if (image != null)
            {
                EndImage.Source = _originalEndImage = image;
                AdaptInputOutputImages();
            }
        }

        /// <summary>
        /// opens a file dialog to load a picture
        /// </summary>
        /// <returns>a new BitmapImage or null if nothing was loaded</returns>
        private BitmapImage LoadImageFileDialog()
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Title = "Select a picture";
            openDialog.Filter = "All supported images|*.jpg;*.jpeg;*.png;*.gif;*.tiff;*.bmp|" +
                                 "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                                 "Portable Network Graphic (*.png)|*.png|" +
                                 "Graphics Interchange Format (*.gif)|*.gif|" +
                                 "Bitmap (*.bmp)|*.bmp|" +
                                 "Tagged Image File Format (*.tiff)|*.tiff";
            if (openDialog.ShowDialog() == true)
            {
                try
                {
                    return new BitmapImage(new Uri(openDialog.FileName));
                }
                catch(Exception)
                {
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// Updates the content of the output image using the fully configurated Morphing-instance and
        /// the current progress from the progress-slider.
        /// </summary>
        private void UpdateOutputImageContent()
        {
            UpdateMarkerCanvases();

            if (OutputImage.Source == null)
                return;

             float progress = (float)((ProgressBar.Value - ProgressBar.Minimum) / (ProgressBar.Maximum - ProgressBar.Minimum));
            _morphingAlgorithm.MorphImages(progress, OutputImage.Source as WriteableBitmap);
        }

        /// <summary>
        /// Rescales the start/end image from the originals to the keep performance at preview-time high.
        /// Then recreates the output image with appropriate size corresponding to scaled StartImage and EndImage
        /// </summary>
        private void AdaptInputOutputImages()
        {
            if (_originalStartImage == null || _originalEndImage == null)
                return;

            StartImage.Source = _originalStartImage;
            StartImage.UpdateLayout();
            StartImage.Source = ImageUtilities.CreateResizedImage(_originalStartImage, (int)(StartImage.ActualWidth), (int)(StartImage.ActualHeight));
            EndImage.Source = _originalEndImage;
            EndImage.UpdateLayout();
            EndImage.Source = ImageUtilities.CreateResizedImage(_originalEndImage, (int)(EndImage.ActualWidth), (int)(EndImage.ActualHeight));

            // create output image
            int width = (int)Math.Max(StartImage.ActualWidth, EndImage.ActualWidth);
            int height = (int)Math.Max(StartImage.ActualHeight, EndImage.ActualHeight);
            OutputImage.Source = new WriteableBitmap(width, height, 0.0f, 0.0f, PixelFormats.Bgra32, null);
            OutputImage.UpdateLayout();
            StartImage.UpdateLayout();
            EndImage.UpdateLayout();

            // setup for morphing
            _morphingAlgorithm.SetStartImage(StartImage.Source as BitmapSource);
            _morphingAlgorithm.SetEndImage(EndImage.Source as BitmapSource);

            // upate output
            if(!_animationPlayer.IsEnabled)
                UpdateOutputImageContent();

            // update marker view
            UpdateMarkerCanvases();
        }

        /// <summary>
        /// Updates the rendering of all canvases with markers.
        /// </summary>
        private void UpdateMarkerCanvases()
        {
            if (_morphingAlgorithm.MarkerSet != null)
            {
                UpdateMarkerCanvases(MarkerSet.Location.START_IMAGE);
                UpdateMarkerCanvases(MarkerSet.Location.END_IMAGE);
                UpdateMarkerCanvases(MarkerSet.Location.OUTPUT_IMAGE);
            }
        }
        private void UpdateMarkerCanvases(MarkerSet.Location location)
        {
            if (_morphingAlgorithm.MarkerSet != null)
            {
                switch(location)
                {
                case MarkerSet.Location.START_IMAGE:
                    _morphingAlgorithm.MarkerSet.UpdateMarkerCanvas(MarkerSet.Location.START_IMAGE, StartImageMarkerCanvas,
                                                                new Vector((StartImageMarkerCanvas.ActualWidth - StartImage.ActualWidth) / 2, (StartImageMarkerCanvas.ActualHeight - StartImage.ActualHeight) / 2),
                                                                new Vector(StartImage.ActualWidth, StartImage.ActualHeight));
                    break;
                case MarkerSet.Location.END_IMAGE:
                    _morphingAlgorithm.MarkerSet.UpdateMarkerCanvas(MarkerSet.Location.END_IMAGE, EndImageMarkerCanvas,
                                                                new Vector((EndImageMarkerCanvas.ActualWidth - EndImage.ActualWidth) / 2, (EndImageMarkerCanvas.ActualHeight - EndImage.ActualHeight) / 2),
                                                                new Vector(EndImage.ActualWidth, EndImage.ActualHeight));
                    break;
                case MarkerSet.Location.OUTPUT_IMAGE:
                    _morphingAlgorithm.MarkerSet.UpdateMarkerCanvas(MarkerSet.Location.OUTPUT_IMAGE, OutputImageMarkerCanvas,
                                                                new Vector((OutputImageMarkerCanvas.ActualWidth - OutputImage.ActualWidth) / 2, (OutputImageMarkerCanvas.ActualHeight - OutputImage.ActualHeight) / 2),
                                                                new Vector(OutputImage.ActualWidth, OutputImage.ActualHeight));
                    break;
                }
            }
        }

        private void OnProgressChange(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateOutputImageContent();
        }

        private Vector ComputeRelativeImagePositionFromMouseEvent(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(sender as IInputElement);
            return new Vector((float)(pos.X / ((Image)sender).ActualWidth), (float)(pos.Y / ((Image)sender).ActualHeight));
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MarkerSet.Location location = sender == StartImage ? MarkerSet.Location.START_IMAGE : MarkerSet.Location.END_IMAGE;
            _morphingAlgorithm.MarkerSet.OnLeftMouseButtonDown(location, ComputeRelativeImagePositionFromMouseEvent(sender, e),
                                                        new Vector(((Image)sender).ActualWidth, ((Image)sender).ActualHeight));
            UpdateOutputImageContent();
        }
        private void Image_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            MarkerSet.Location location = sender == StartImage ? MarkerSet.Location.START_IMAGE : MarkerSet.Location.END_IMAGE;
            _morphingAlgorithm.MarkerSet.OnRightMouseButtonDown(location, ComputeRelativeImagePositionFromMouseEvent(sender, e),
                                                        new Vector(((Image)sender).ActualWidth, ((Image)sender).ActualHeight));
            UpdateOutputImageContent();
        }
        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            MarkerSet.Location location = sender == StartImage ? MarkerSet.Location.START_IMAGE : MarkerSet.Location.END_IMAGE;
            bool changedMarker = _morphingAlgorithm.MarkerSet.OnMouseMove(location, ComputeRelativeImagePositionFromMouseEvent(sender, e),
                                                        new Vector(((Image)sender).ActualWidth, ((Image)sender).ActualHeight));
            if (!_animationPlayer.IsEnabled && changedMarker)
                UpdateOutputImageContent();
            else
                UpdateMarkerCanvases(location);
        }
      
        private void Image_MarkerDeselect(object sender, MouseEventArgs e)
        {
            _morphingAlgorithm.MarkerSet.OnLeftMouseButtonUp();
            UpdateMarkerCanvases();
        }

        /// <summary>
        /// window was resized
        /// </summary>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdaptInputOutputImages();
            UpdateMarkerCanvases();
        }

        /// <summary>
        /// another morphing algorithm was choosen
        /// </summary>
        private void MorphingTechniqueSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox != null)
            {
                _morphingAlgorithm.AlgorithmType = (Morphing.Algorithm)comboBox.SelectedIndex;
                UpdateMarkerCanvases();
            }
        }

        /// <summary>
        /// click on one of the dissolver setting radioboxes
        /// </summary>
        private void DissolverSettingChanged(object sender, RoutedEventArgs e)
        {
            if((bool)DissolverSetting_Combined.IsChecked)
                _morphingAlgorithm.DissolverType = Morphing.Dissolver.ALPHABLEND;
            else if ((bool)DissolverSetting_StartOnly.IsChecked)
                _morphingAlgorithm.DissolverType = Morphing.Dissolver.SELECT_START;
            else if ((bool)DissolverSetting_EndOnly.IsChecked)
                _morphingAlgorithm.DissolverType = Morphing.Dissolver.SELECT_END;

            UpdateOutputImageContent();
        }
        
        /// <summary>
        /// click on the render video button
        /// </summary>
        private void RenderVideo_Click(object sender, RoutedEventArgs e)
        {
            if (_originalStartImage == null || _originalStartImage == null)
                return;

            _morphingAlgorithm.SetStartImage(_originalStartImage);
            _morphingAlgorithm.SetEndImage(_originalEndImage);
            try
            {
                System.IO.Directory.Delete("temp", true);
            }
            catch(Exception) {}
            System.IO.Directory.CreateDirectory("temp");

            // buffer
     /*       const int BUFFER_SIZE = 4;
            WriteableBitmap[] bitmapBuffer = new WriteableBitmap[BUFFER_SIZE];
            for(int i=0; i<BUFFER_SIZE; ++i)
            {
                bitmapBuffer[i] = new WriteableBitmap(Math.Max(_originalStartImage.PixelWidth, _originalEndImage.PixelWidth),
                                                            Math.Max(_originalStartImage.PixelHeight, _originalEndImage.PixelHeight),
                                                            96.0f, 96.0f, PixelFormats.Bgra32, null);
            }
            System.Threading.Semaphore usableImageIndex = new System.Threading.Semaphore(0, BUFFER_SIZE); 
            */

            var output = new WriteableBitmap(Math.Max(_originalStartImage.PixelWidth, _originalEndImage.PixelWidth),
                                                            Math.Max(_originalStartImage.PixelHeight, _originalEndImage.PixelHeight),
                                                            96.0f, 96.0f, PixelFormats.Bgra32, null);

            int numFrames = (int)NumFrames.Value;
            for (int frame = 0; frame < numFrames; ++frame)
            {
                float progress = (float)frame / (numFrames - 1);
                _morphingAlgorithm.MorphImages(progress, output);

                // save to file
                string filename = "temp//frame" + (frame+1) + ".jpg";
                var imageEncoder = new JpegBitmapEncoder();
                imageEncoder.QualityLevel = 70;
                imageEncoder.Frames.Add(BitmapFrame.Create(output));
                using (var file = System.IO.File.OpenWrite(filename))
                    imageEncoder.Save(file);
                imageEncoder.Frames.Clear();
            }

            // encode
            int frameRate = (int)(numFrames / (double)Duration.Value + 0.5);
            System.Diagnostics.Process.Start("ffmpeg.exe", "-f image2 -i temp//frame%d.jpg -r " + frameRate + " video.mpg");

            _morphingAlgorithm.SetStartImage(StartImage.Source as BitmapSource);
            _morphingAlgorithm.SetEndImage(EndImage.Source as BitmapSource);
        }

        /// <summary>
        /// Click on the show output marker checkbox
        /// </summary>
        private void ShowOutputMarker_Checked(object sender, RoutedEventArgs e)
        {
            OutputImageMarkerCanvas.Visibility = (bool)((CheckBox)sender).IsChecked ? Visibility.Visible : Visibility.Hidden;
            OutputImageMarkerCanvas.UpdateLayout();
            UpdateMarkerCanvases();
        }

        /// <summary>
        /// Click on the clear marker button
        /// </summary>
        private void OnClearMarker_Click(object sender, RoutedEventArgs e)
        {
            _morphingAlgorithm.MarkerSet.ClearMarkers();
            UpdateMarkerCanvases();
        }
    }
}
