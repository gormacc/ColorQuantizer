using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

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


            int height = imageToQuantizeBitmap.Height;
            int width = imageToQuantizeBitmap.Width;

            int colorCount;

            if (!int.TryParse(PixelCountTextBox.Text, out colorCount)) return;

            OctreeQuantizerBase octree;

            if (!withInstantReduction)
            {
                octree = new OctreeQuantizerNormal(colorCount);
            }
            else
            {
                octree = new OctreeQuantizerInstantReduction(colorCount);
            }

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Color pixel = imageToQuantizeBitmap.GetPixel(i, j);
                    octree.AddColor(new ColorRgb(pixel.R, pixel.G, pixel.B));
                }
            }

            List<System.Windows.Media.Color> palette = octree.MakePalette();

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

            if (!withInstantReduction)
            {
                QuantizerNormalImage.Source = ConvertBitmapToBitmapImage(outBitmap);
            }
            else
            {
                QuantizerInstantReductionImage.Source = ConvertBitmapToBitmapImage(outBitmap);
            }
        }

        public Bitmap ConvertImageToBitmap(BitmapImage bitmapImage)
        {
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap, (int)bitmapImage.Width, (int)bitmapImage.Height);
            }
        }

        public BitmapImage ConvertFileToBitmapImage(string fileNameOrPath, bool isFullPath)
        {
            BitmapImage bmp = new BitmapImage();
            try
            {
                bmp.BeginInit();
                bmp.UriSource = isFullPath ? new Uri(fileNameOrPath) : new Uri(Path.Combine(Directory.GetCurrentDirectory(), fileNameOrPath));
                bmp.EndInit();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            return bmp;
        }

        public BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        private void LoadImage(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg)|*.png;*.jpeg|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();

            if (openFileDialog.ShowDialog() == true)
            {
                imageToQuantizeBitmap = ConvertImageToBitmap(ConvertFileToBitmapImage(openFileDialog.FileName, true));
                ImageToQuantize.Source = ConvertBitmapToBitmapImage(imageToQuantizeBitmap);
            }
        }
    }
}
