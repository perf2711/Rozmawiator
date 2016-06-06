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

namespace Rozmawiator.PartialViews
{
    /// <summary>
    /// Interaction logic for SearchUsersControl.xaml
    /// </summary>
    public partial class SearchUsersControl : UserControl
    {
        public SearchUsersControl()
        {
            InitializeComponent();
        }

        public void Search()
        {
            var query = SearchBox.Text;

            
        }

        

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Search();
            }
        }
    }
}
