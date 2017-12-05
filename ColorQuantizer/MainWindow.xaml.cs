using Microsoft.Win32;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;

namespace ColorQuantizer
{
    public partial class MainWindow : Window
    {
        private Bitmap imageToQuantizeBitmap;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void QuantizeNormal(object sender, RoutedEventArgs e)
        {
            Quantize(false);
        }

        public void QuantizeInstantReduction(object sender, RoutedEventArgs e)
        {
            Quantize(true);
        }

        private void Quantize(bool withInstantReduction)
        {
            if (imageToQuantizeBitmap == null) return;

            OctreeQuantizerBase octree = InitializeOctree(withInstantReduction);

            List<System.Windows.Media.Color> palette = MakePalette(octree);

            Bitmap outBitmap = RewriteImageWithPalette(octree, palette);

            ShowBitmap(outBitmap, withInstantReduction);
        }

        private OctreeQuantizerBase InitializeOctree(bool withInstantReduction)
        {
            int colorCount;

            if (!int.TryParse(PixelCountTextBox.Text, out colorCount))
            {
                colorCount = 64;
            }

            if (!withInstantReduction)
            {
                return new OctreeQuantizerNormal(colorCount);
            }
            else
            {
                return new OctreeQuantizerInstantReduction(colorCount);
            }
        }

        private List<System.Windows.Media.Color> MakePalette(OctreeQuantizerBase octree)
        {
            int height = imageToQuantizeBitmap.Height;
            int width = imageToQuantizeBitmap.Width;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Color pixel = imageToQuantizeBitmap.GetPixel(i, j);
                    octree.AddColor(new ColorRgb(pixel.R, pixel.G, pixel.B));
                }
            }

            return octree.MakePalette();
        }

        private Bitmap RewriteImageWithPalette(OctreeQuantizerBase octree, List<System.Windows.Media.Color> palette)
        {
            int height = imageToQuantizeBitmap.Height;
            int width = imageToQuantizeBitmap.Width;

            Bitmap outBitmap = new Bitmap(width, height);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Color pixel = imageToQuantizeBitmap.GetPixel(i, j);
                    int index = octree.GetPalletteIndex(new ColorRgb(pixel.R, pixel.G, pixel.B));

                    System.Windows.Media.Color color = palette[index];
                    outBitmap.SetPixel(i, j, Color.FromArgb(color.A, color.R, color.G, color.B));
                }
            }

            return outBitmap;
        }

        private void ShowBitmap(Bitmap bitmap, bool withInstantReduction)
        {
            if (!withInstantReduction)
            {
                QuantizerNormalImage.Source = ImageHelper.ConvertBitmapToBitmapImage(bitmap);
            }
            else
            {
                QuantizerInstantReductionImage.Source = ImageHelper.ConvertBitmapToBitmapImage(bitmap);
            }
        }

        private void LoadImage(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.bmp;*.jpg;*.gif)|*.png;*.jpeg;*.bmp;*.jpg;*.gif|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();

            if (openFileDialog.ShowDialog() == true)
            {
                imageToQuantizeBitmap = ImageHelper.ConvertImageToBitmap(ImageHelper.ConvertFileToBitmapImage(openFileDialog.FileName, true));
                ImageToQuantize.Source = ImageHelper.ConvertBitmapToBitmapImage(imageToQuantizeBitmap);
            }
        }
    }
}
