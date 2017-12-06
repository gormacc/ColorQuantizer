using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace ColorQuantizer
{
    public partial class MainWindow : Window
    {
        private Bitmap _imageToQuantizeBitmap;
        private BackgroundWorker _backgroundWorkerOne = new BackgroundWorker();
        private BackgroundWorker _backgroundWorkerTwo = new BackgroundWorker();
        private Bitmap _backgroundWorkerOneBitmap;
        private Bitmap _backgroundWorkerTwoBitmap;
        private Action GetColorCountAction;
        public int ColorCount { get; set; } = 64;

        public MainWindow()
        {
            InitializeComponent();
            InitializeBackgroundWorkers();
        }

        private void InitializeBackgroundWorkers()
        {


            _backgroundWorkerOne.WorkerReportsProgress = true;
            _backgroundWorkerTwo.WorkerReportsProgress = true;

            _backgroundWorkerOne.WorkerSupportsCancellation = false;
            _backgroundWorkerTwo.WorkerSupportsCancellation = false;

            _backgroundWorkerOne.DoWork += QuantizeNormal;
            _backgroundWorkerTwo.DoWork += QuantizeInstantReduction;

            _backgroundWorkerOne.ProgressChanged += QuantizeNormalProgress;
            _backgroundWorkerTwo.ProgressChanged += QuantizeInstantReductionProgress;

            _backgroundWorkerOne.RunWorkerCompleted += EndQuantizeNormal;
            _backgroundWorkerTwo.RunWorkerCompleted += EndQuantizeInstantReduction;
        }

        private void QuantizeNormal(object sender, DoWorkEventArgs e)
        {
            if (_imageToQuantizeBitmap == null) return;

            Quantize(false, new Bitmap(_imageToQuantizeBitmap));
        }

        private void QuantizeInstantReduction(object sender, DoWorkEventArgs e)
        {
            if (_imageToQuantizeBitmap == null) return;

            Quantize(true, new Bitmap(_imageToQuantizeBitmap));
        }

        private void QuantizeNormalProgress(object sender, ProgressChangedEventArgs e)
        {
            QuantizerNormalProgressBar.Value = e.ProgressPercentage;
        }

        private void QuantizeInstantReductionProgress(object sender, ProgressChangedEventArgs e)
        {
            QuantizerInstantReductionProgressBar.Value = e.ProgressPercentage;
        }

        private void EndQuantizeNormal(object sender, RunWorkerCompletedEventArgs e)
        {
            ShowBitmap(false);
            QuantizeNormalButton.IsEnabled = true;
        }

        private void EndQuantizeInstantReduction(object sender, RunWorkerCompletedEventArgs e)
        {
            ShowBitmap(true);
            QuantizeInstantReductionButton.IsEnabled = true;
        }

        public void QuantizeNormalClick(object sender, RoutedEventArgs e)
        {
            if (!_backgroundWorkerOne.IsBusy)
            {
                QuantizeNormalButton.IsEnabled = false;
                _backgroundWorkerOne.RunWorkerAsync();
            }            
        }

        public void QuantizeInstantReductionClick(object sender, RoutedEventArgs e)
        {
            if (!_backgroundWorkerTwo.IsBusy)
            {
                QuantizeInstantReductionButton.IsEnabled = false;
                _backgroundWorkerTwo.RunWorkerAsync();
            }
        }

        private void Quantize(bool withInstantReduction, Bitmap bitmap)
        {
            if (bitmap == null) return;

            OctreeQuantizerBase octree = InitializeOctree(withInstantReduction,ColorCount);

            List<System.Windows.Media.Color> palette = MakePalette(octree, bitmap, withInstantReduction);

            RewriteImageWithPalette(octree, palette, bitmap, withInstantReduction);
        }

        private OctreeQuantizerBase InitializeOctree(bool withInstantReduction, int colorCount)
        {           
            if (!withInstantReduction)
            {
                return new OctreeQuantizerNormal(colorCount);
            }
            else
            {
                return new OctreeQuantizerInstantReduction(colorCount);
            }
        }

        private List<System.Windows.Media.Color> MakePalette(OctreeQuantizerBase octree, Bitmap bitmap, bool withInstantReduction)
        {
            int height = bitmap.Height;
            int width = bitmap.Width;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Color pixel = bitmap.GetPixel(i, j);
                    octree.AddColor(new ColorRgb(pixel.R, pixel.G, pixel.B));
                }
                ReportProgressToWorker(withInstantReduction, 100 * i / width);
            }

            return octree.MakePalette();
        }

        private void RewriteImageWithPalette(OctreeQuantizerBase octree, List<System.Windows.Media.Color> palette, Bitmap bitmap, bool withInstantReduction)
        {
            int height = bitmap.Height;
            int width = bitmap.Width;

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
                ReportProgressToWorker(withInstantReduction, 100 * i / width);
            }

            SetQuantizedBitmap(withInstantReduction, outBitmap);
        }

        private void ReportProgressToWorker(bool withInstantReduction, int percentage)
        {
            if (!withInstantReduction)
            {
                _backgroundWorkerOne.ReportProgress(percentage);
            }
            else
            {
                _backgroundWorkerTwo.ReportProgress(percentage);
            }
        }

        private void SetQuantizedBitmap(bool withInstantReduction, Bitmap bitmap)
        {
            if (!withInstantReduction)
            {
                _backgroundWorkerOneBitmap = bitmap;
            }
            else
            {
                _backgroundWorkerTwoBitmap = bitmap;
            }
        }

        private void ShowBitmap(bool withInstantReduction)
        {
            if (!withInstantReduction)
            {
                QuantizerNormalImage.Source = ImageHelper.ConvertBitmapToBitmapImage(_backgroundWorkerOneBitmap);
            }
            else
            {
                QuantizerInstantReductionImage.Source = ImageHelper.ConvertBitmapToBitmapImage(_backgroundWorkerTwoBitmap);
            }
        }

        private void LoadImage(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.bmp;*.jpg;*.gif)|*.png;*.jpeg;*.bmp;*.jpg;*.gif|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();

            if (openFileDialog.ShowDialog() == true)
            {
                _imageToQuantizeBitmap = ImageHelper.ConvertImageToBitmap(ImageHelper.ConvertFileToBitmapImage(openFileDialog.FileName, true));
                ImageToQuantize.Source = ImageHelper.ConvertBitmapToBitmapImage(_imageToQuantizeBitmap);
            }
        }
    }
}
