using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ColorQuantizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void QuantizeNormal(object sender, RoutedEventArgs e)
        {
            Bitmap bitmap = ConvertImageToBitmap(ConvertFileToBitmapImage("test2.jpg", false));
            int height = bitmap.Height;
            int width = bitmap.Width;

            int colorCount;

            if (!int.TryParse(PixelCountTextBox.Text, out colorCount)) return;

            OctreeQuantizerNormal octree = new OctreeQuantizerNormal(colorCount);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Color pixel = bitmap.GetPixel(i, j);
                    octree.AddColor(new ColorRgb(pixel.R, pixel.G, pixel.B));
                }
            }

            List<System.Windows.Media.Color> palette = octree.MakePalette();

            Bitmap outBitmap = new Bitmap(width, height);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Color pixel = bitmap.GetPixel(i, j);
                    int index = octree.GetPalletteIndex(new ColorRgb(pixel.R, pixel.G, pixel.B));

                    System.Windows.Media.Color color = palette[index];
                    outBitmap.SetPixel(i,j, Color.FromArgb(color.A, color.R, color.G, color.B));
                }
            }           

            ShowImageWindow window = new ShowImageWindow(ConvertBitmapToBitmapImage(outBitmap));
            window.Show();

            ShowColorWindow(palette);
        }

        public void QuantizeInstantReduction(object sender, RoutedEventArgs e)
        {
            Bitmap bitmap = ConvertImageToBitmap(ConvertFileToBitmapImage("test.jpg", false));
            int height = bitmap.Height;
            int width = bitmap.Width;

            int colorCount;

            if (!int.TryParse(PixelCountTextBox.Text, out colorCount)) return;

            OctreeQuantizerInstantReduction octree = new OctreeQuantizerInstantReduction(colorCount);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Color pixel = bitmap.GetPixel(i, j);
                    octree.AddColor(new ColorRgb(pixel.R, pixel.G, pixel.B));
                }
            }

            List<System.Windows.Media.Color> palette = octree.MakePalette();

            Bitmap outBitmap = new Bitmap(width, height);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Color pixel = bitmap.GetPixel(i, j);
                    int index = octree.GetPalletteIndex(new ColorRgb(pixel.R, pixel.G, pixel.B));

                    System.Windows.Media.Color color = palette[index];
                    outBitmap.SetPixel(i, j, Color.FromArgb(color.A, color.R, color.G, color.B));
                }
            }

            ShowImageWindow window = new ShowImageWindow(ConvertBitmapToBitmapImage(outBitmap));
            window.Show();

            ShowColorWindow(palette);
        }

        private void ShowColorWindow(List<System.Windows.Media.Color> palette)
        {
            //Bitmap colorBitmap = new Bitmap(4, 4);

            //for (int i = 0; i < palette.Count; i++)
            //{
            //    int a = i / 4;
            //    int b = i % 4;
            //    var color = palette[i];
            //    colorBitmap.SetPixel(a, b, Color.FromArgb(color.A, color.R, color.G, color.B));
            //}

            //ShowImageWindow window = new ShowImageWindow(ConvertBitmapToBitmapImage(colorBitmap));
            //window.Show();
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
    }
}
