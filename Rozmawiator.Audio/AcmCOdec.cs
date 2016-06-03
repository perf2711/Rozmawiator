using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NAudio;
using NAudio.Wave;
using NAudio.Wave.Compression;

namespace Rozmawiator.Audio
{
    public abstract class AcmCodec
    {
        public WaveFormat RecordFormat { get; }
        public string Name { get; protected set; }

        private readonly WaveFormat _encodeFormat;
        private AcmStream _encoder;
        private AcmStream _decoder;
        private int _decodeSourceBytesLeftovers;
        private int _encodeSourceBytesLeftovers;

        public int BitsPerSecond => _encodeFormat.AverageBytesPerSecond * 8;

        public bool IsAvailable
        {
            get
            {
                // determine if this codec is installed on this PC
                try
                {
                    using (new AcmStream(RecordFormat, _encodeFormat)) { }
                    using (new AcmStream(_encodeFormat, RecordFormat)) { }
                }
                catch (MmException)
                {
                    return false;
                }
                return true;
            }
        }

        protected AcmCodec(WaveFormat recordFormat, WaveFormat encodeFormat)
        {
            RecordFormat = recordFormat;
            _encodeFormat = encodeFormat;
            _encoder = new AcmStream(RecordFormat, _encodeFormat);
        }

        public byte[] Encode(byte[] data, int offset, int length)
        {
            return Convert(_encoder, data, offset, length, ref _encodeSourceBytesLeftovers);
        }

        public byte[] Decode(byte[] data, int offset, int length)
        {
            return Convert(_decoder, data, offset, length, ref _decodeSourceBytesLeftovers);
        }

        private static byte[] Convert(AcmStream conversionStream, byte[] data, int offset, int length, ref int sourceBytesLeftovers)
        {
            var bytesInSourceBuffer = length + sourceBytesLeftovers;
            Array.Copy(data, offset, conversionStream.SourceBuffer, sourceBytesLeftovers, length);
            int sourceBytesConverted;
            var bytesConverted = conversionStream.Convert(bytesInSourceBuffer, out sourceBytesConverted);
            sourceBytesLeftovers = bytesInSourceBuffer - sourceBytesConverted;
            if (sourceBytesLeftovers > 0)
            {
                //Debug.WriteLine(String.Format("Asked for {0}, converted {1}", bytesInSourceBuffer, sourceBytesConverted));
                // shift the leftovers down
                Array.Copy(conversionStream.SourceBuffer, sourceBytesConverted, conversionStream.SourceBuffer, 0, sourceBytesLeftovers);
            }
            byte[] encoded = new byte[bytesConverted];
            Array.Copy(conversionStream.DestBuffer, 0, encoded, 0, bytesConverted);
            return encoded;
        }

        public void Dispose()
        {
            if (_encoder != null)
            {
                _encoder.Dispose();
                _encoder = null;
            }
            if (_decoder != null)
            {
                _decoder.Dispose();
                _decoder = null;
            }
        }
    }
}
