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
            _waveOut.Play();
        }

        public void Stop()
        {
            _waveOut.Stop();
        }
    }
}
