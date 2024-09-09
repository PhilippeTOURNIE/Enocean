using System.Collections;

namespace Enocean.Core
{
    public static class Utils
    {
        public static uint CombineHex(List<byte> data)
        {
            // Combine list of integer values to one big integer
            uint output = 0x00;
            for (int i = 0; i < data.Count; i++)
            {
                uint value = data[data.Count - 1 - i];
                output |= (value << i * 8);
            }
            return output;
        }

        public static BitArray ToBitArray(byte[] data)
        {
            return new BitArray(data.Reverse().ToArray()).Reverse();
        }

        public static byte FromBitArray(BitArray data)
        {
            int[] array = new int[1];
            data.Reverse().CopyTo(array, 0);
            return (byte)array[0];
        }
        public static List<byte> FromBiteArray(this BitArray data)
        {
            var dataReverse = data.Reverse();
            List<byte> res = new List<byte>();
            for (int i = 0; i < data.Length; i += 8)
            {
                int[] array = new int[1];
                dataReverse.Skip(i).CopyTo(array, 0);
                res.Add((byte)array[0]);
            }
            res.Reverse();
            return res;
        }
        public static string ToHexString(byte data)
        {
            return string.Format("{0:X2}", data);
        }
        public static string ToHexString(List<byte> data)
        {
            return string.Join(":", data.Select(o => string.Format("{0:X2}", o)));
        }
        public static int FromHexStringToInt(string hexString)
        {
            var reval = hexString.Split(':').Select(x => Convert.ToInt32(x, 16)).ToList();
            return reval[0];
        }
        public static BitArray Skip(this BitArray array, int value)
        {
            bool[] bools = new bool[array.Count];
            array.CopyTo(bools, 0);
            return new BitArray(bools.Skip(value).ToArray());
        }
        public static BitArray Take(this BitArray array, int value)
        {
            bool[] bools = new bool[array.Count];
            array.CopyTo(bools, 0);
            return new BitArray(bools.Take(value).ToArray());
        }
        public static BitArray Concat(this BitArray bitArray, BitArray bitArray1)
        {
            bool[] bools = new bool[bitArray.Length];
            bitArray.CopyTo(bools, 0);
            bool[] bools1 = new bool[bitArray1.Length];
            bitArray1.CopyTo(bools1, 0);
            return new BitArray(bools.Concat(bools1).ToArray());
        }
        public static BitArray Reverse(this BitArray array)
        {
            int length = array.Length;
            int mid = (length / 2);

            for (int i = 0; i < mid; i++)
            {
                bool bit = array[i];
                array[i] = array[length - i - 1];
                array[length - i - 1] = bit;
            }

            return new BitArray(array);
        }
    }
}
