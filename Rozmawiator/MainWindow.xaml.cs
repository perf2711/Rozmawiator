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
using Rozmawiator.Audio;

namespace Rozmawiator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Recorder _recorder;
        private readonly Player _player;

        public MainWindow()
        {
            InitializeComponent();

            _recorder = new Recorder();
            _recorder.DataAvailable += RecorderOnDataAvailable;

            _player = new Player();
        }

        private void RecorderOnDataAvailable(Recorder recorder, byte[] waveInEventArgs)
        {
            _player.AddSamples(waveInEventArgs, 0, waveInEventArgs.Length);
        }

        private void recordButton_Click(object sender, RoutedEventArgs e)
        {
            switch (_recorder.State)
            {
                case Recorder.RecorderState.Idle:
                    _player.Start();
                    _recorder.Start();
                    break;

                case Recorder.RecorderState.Recording:
                    _player.Stop();
                    _recorder.Stop();
                    break;
            }
        }
    }
}
