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
    /// Interaction logic for LoadingControl.xaml
    /// </summary>
    public partial class LoadingControl : UserControl
    {
        public string Text
        {
            get { return (string) LoadingLabel.GetValue(ContentProperty); }
            set { LoadingLabel.SetValue(ContentProperty, value); }
        }

        public LoadingControl()
        {
            InitializeComponent();
        }
    }
}
