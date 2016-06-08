using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace Rozmawiator.Audio
{
    public class Recorder
    {
        public enum RecorderState
        {
            Idle,
            Recording
        }

        public RecorderState State { get; private set; } = RecorderState.Idle;

        private readonly WaveIn _waveIn;
        private readonly AcmCodec _codec = new Gsm610Codec();

        public event Action<Recorder, byte[]> DataAvailable;

        public Recorder()
        {
            _waveIn = new WaveIn();
            _waveIn.WaveFormat = _codec.RecordFormat;
            _waveIn.DataAvailable += OnNewData;
        }

        public void Start()
        {
            if (State == RecorderState.Recording)
            {
                return;
            }

            _waveIn.StartRecording();
            State = RecorderState.Recording;
        }

        public void Stop()
        {
            if (State == RecorderState.Idle)
            {
                return;
            }

            _waveIn.StopRecording();
            State = RecorderState.Idle;
        }

        private void OnNewData(object sender, WaveInEventArgs waveInEventArgs)
        {
            DataAvailable?.Invoke(this, waveInEventArgs.Buffer);
        }
    }
}
