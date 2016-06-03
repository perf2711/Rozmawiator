using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace Rozmawiator.Audio
{
    public class Gsm610Codec : AcmCodec
    {
        public Gsm610Codec() : base(new WaveFormat(8000, 16, 1), new Gsm610WaveFormat())
        {
            Name = "GSM 6.10";
        }
    }
}
