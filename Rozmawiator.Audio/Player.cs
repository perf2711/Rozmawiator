using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace Rozmawiator.Audio
{
    public class Player
    {
        public enum PlayerState
        {
            Stopped,
            Playing,
            Paused
        }

        public PlayerState State => (PlayerState)_waveOut.PlaybackState;

        private readonly WaveOut _waveOut;
        private readonly AcmCodec _codec = new Gsm610Codec();
        private readonly BufferedWaveProvider _waveProvider;

        public float Volume
        {
            get { return _waveOut.Volume; }
            set { _waveOut.Volume = Math.Min(Math.Max(0, value), 1); }
        }

        public void SetMute(bool mute, float volume = 1)
        {
            Volume = mute ? 0 : volume;
        }

        public Player()
        {
            _waveProvider = new BufferedWaveProvider(_codec.RecordFormat);
            _waveProvider.DiscardOnBufferOverflow = true;

            _waveOut = new WaveOut();
            _waveOut.Init(_waveProvider);
        }

        public void AddSamples(byte[] samples, int offset, int count)
        {
            _waveProvider.AddSamples(samples, offset, count);
        }

        public void Start()
        {
            if (State == PlayerState.Stopped)
            {
                SetMute(false);
                _waveOut.Play();
            }
        }

        public void Stop()
        {
            if (State != PlayerState.Stopped)
            {
                SetMute(true);
                _waveOut.Stop();
            }
        }
    }
}
