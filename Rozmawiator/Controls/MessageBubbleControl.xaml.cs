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
    /// Interaction logic for MessageBubble.xaml
    /// </summary>
    public partial class MessageBubbleControl : UserControl
    {
        #region Properties

        public new Brush Background
        {
            get { return Resources["Background"] as Brush; }
            set { Resources["Background"] = value; }
        }

        public new Brush Foreground
        {
            get { return Resources["Foreground"] as Brush; }
            set { Resources["Background"] = value; }
        }

        public new string Content
        {
            get { return MessageTextBox.GetValue(TextBox.TextProperty) as string; }
            set { MessageTextBox.SetValue(TextBox.TextProperty, value);}
        }

        #endregion

        public MessageBubbleControl()
        {
            InitializeComponent();
        }
    }
}
