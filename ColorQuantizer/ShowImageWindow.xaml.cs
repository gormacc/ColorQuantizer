using System.Windows;
using System.Windows.Media.Imaging;

namespace ColorQuantizer
{
    /// <summary>
    /// Interaction logic for ShowImageWindow.xaml
    /// </summary>
    public partial class ShowImageWindow : Window
    {
        public ShowImageWindow(BitmapImage image)
        {
            InitializeComponent();

            Height = image.Height;
            Width = image.Width;
            ImageToShow.Source = image;
        }
    }
}
