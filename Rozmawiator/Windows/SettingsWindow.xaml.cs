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
using NAudio.Wave;

namespace Rozmawiator.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public bool SettingsChanged { get; private set; } = false;

        public SettingsWindow()
        {
            InitializeComponent();

            RefreshDevices();
        }

        private void RefreshDevices()
        {
            var playDevices = Enumerable.Range(0, WaveIn.DeviceCount).Select(id => new { Id = id, Device = WaveIn.GetCapabilities(id) });
            var recordDevices = Enumerable.Range(0, WaveOut.DeviceCount).Select(id => new { Id = id, Device = WaveOut.GetCapabilities(id) });

            foreach (var playDevice in playDevices)
            {
                var item = new ComboBoxItem
                {
                    Content = playDevice.Device.ProductName,
                    Tag = playDevice.Id
                };

                PlayDeviceBox.Items.Add(item);
            }

            foreach (var recordDevice in recordDevices)
            {
                var item = new ComboBoxItem
                {
                    Content = recordDevice.Device.ProductName,
                    Tag = recordDevice.Id
                };

                RecordDeviceBox.Items.Add(item);
            }

            PlayDeviceBox.SelectedIndex = 0;
            RecordDeviceBox.SelectedIndex = 0;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.PlayDeviceId = (PlayDeviceBox.SelectedItem as ComboBoxItem)?.Tag as int? ?? -1;
            Properties.Settings.Default.RecordDeviceId = (RecordDeviceBox.SelectedItem as ComboBoxItem)?.Tag as int? ?? -1;
            Properties.Settings.Default.Save();

            SettingsChanged = true;

            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
