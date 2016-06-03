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
using System.Windows.Shapes;
using Rozmawiator.Server.Controls;

namespace Rozmawiator.Server.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private Type[] _configSections;

        public SettingsWindow(params Type[] configSections)
        {
            InitializeComponent();
            _configSections = configSections;

            foreach (var section in configSections)
            {
                AddSection(section);
            }
        }

        public void AddSection(Type configSection)
        {
            var section = new SettingsSectionControl(configSection);
            section.Title = configSection.Name;
            SettingsGrid.Children.Add(section);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsGrid.Children.OfType<SettingsSectionControl>().Any(sectionControl => !sectionControl.UpdateSection()))
            {
                MessageBox.Show("Failed to save config. Check if types correspond to their values.");
                return;
            }

            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
