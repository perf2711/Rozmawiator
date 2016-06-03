using System;

namespace Rozmawiator.Shared
{
    public static class NumericalExtensions
    {
        public static byte[] GetBytes(this short value)
        {
            return new[] { (byte)value, (byte)(value >> 8) };
        }

        public static short GetShort(this byte[] array)
        {
            if (array.Length != 2)
            {
                throw new InvalidOperationException("Invalid array length.");
            }

            return (short)((short)(array[1] << 8) + array[0]);
        }
    }
}
