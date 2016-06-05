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
    /// Interaction logic for ErrorControl.xaml
    /// </summary>
    public partial class ErrorControl : UserControl
    {
        public event Action<ErrorControl> CloseClick;

        public string ErrorMessage
        {
            get { return (string) ContentLabel.GetValue(ContentProperty); }
            set { ContentLabel.SetValue(ContentProperty, value); }
        }

        public string ErrorHeader
        {
            get { return (string) HeaderLabel.GetValue(ContentProperty); }
            set { HeaderLabel.SetValue(ContentProperty, value); }
        }

        public Brush ErrorHeaderBackground
        {
            get { return (Brush) HeaderGrid.GetValue(BackgroundProperty); }
            set { HeaderGrid.SetValue(BackgroundProperty, value); }
        }

        public ErrorControl()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var panel = Parent as Panel;
            panel?.Children.Remove(this);
            CloseClick?.Invoke(this);
        }
    }
}
