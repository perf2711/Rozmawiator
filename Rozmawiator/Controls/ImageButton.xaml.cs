using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Rozmawiator.Controls
{
    /// <summary>
    /// Interaction logic for ImageButton.xaml
    /// </summary>
    public partial class ImageButton : UserControl
    {
        public ImageSource Icon
        {
            get { return (ImageSource) Image.GetValue(Image.SourceProperty); }
            set { Image.SetValue(Image.SourceProperty, value); }
        }

        public ImageSource IconDim
        {
            get { return (ImageSource)ImageDim.GetValue(Image.SourceProperty); }
            set { ImageDim.SetValue(Image.SourceProperty, value); }
        }

        public event Action<ImageButton, RoutedEventArgs> Click;

        public ImageButton()
        {
            InitializeComponent();
        }

        private void MainButton_MouseEnter(object sender, MouseEventArgs e)
        {
            ImageDim.Opacity = 0;
        }

        private void MainButton_MouseLeave(object sender, MouseEventArgs e)
        {
            ImageDim.Opacity = 0.5;
        }

        protected virtual void OnClick(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, e);
        }

        private void MainButton_Click(object sender, RoutedEventArgs e)
        {
            OnClick(sender, e);
        }
    }
}
