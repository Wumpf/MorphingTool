using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace MorphingTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Morphing _morphingAlgorithm = new Morphing();
    
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// eventhandler for clicking on the loadstartimage button
        /// </summary>
        private void OnClick_LoadStartImage(object sender, RoutedEventArgs e)
        {
            BitmapImage image = LoadImageFileDialog();
            if (image != null)
            {
                StartImage.Source = image;
                RecreateOutputImage();
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
                EndImage.Source = image;
                RecreateOutputImage();
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
            if (StartImage.Source == null || EndImage.Source == null)
                return;

            _morphingAlgorithm.MorphImages(StartImage.Source, EndImage.Source, 0.0f, OutputImage.Source as WriteableBitmap);
        }

        /// <summary>
        /// Recreates the output image with appropriate size corresponding to StartImage and OutputImage
        /// </summary>
        private void RecreateOutputImage()
        {
            if (StartImage.Source == null || EndImage.Source == null)
                return;

            int width = (int)Math.Max(StartImage.Source.Width, EndImage.Source.Width);
            int height = (int)Math.Max(StartImage.Source.Height, EndImage.Source.Height);
            
            OutputImage.Source = new WriteableBitmap(width, height, 0.0f, 0.0f, PixelFormats.Bgra32, null);
            UpdateOutputImageContent();
        }
    }
}
