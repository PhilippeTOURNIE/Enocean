using Enocean.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Enocean.Test
{
    [TestClass]
    public class UtilTest
    {
        [TestMethod,Timeout(100)]
        public void ToBitArray()
        {
            var bitArray = Utils.ToBitArray(new byte[] { 0x00, 0x00, 0x55, 0x08 });
            string binaryString = "";
            var b =bitArray.Skip(16).Take(8);
            for (int i = 0; i < b.Length; i++)
            {
                binaryString += b[i] ? '1' : '0';
            }

            int t = Convert.ToInt32(binaryString, 2);

            Assert.IsTrue(t == 85);
        }

        [TestMethod,Timeout(100)]
        public void FromBitArray()
        {
            var bitArray = Utils.ToBitArray(new byte[] { 0x30 });

            var res=Utils.FromBitArray(bitArray);
             Assert.IsTrue(res == 0x30);
        }

        [TestMethod,Timeout(100)]
        public void FromBitArray2()
        {
            var bitArray = Utils.ToBitArray(new byte[] { 0x00, 0x10, 0x55, 0x08 });
            var res = bitArray.FromBiteArray();

            Assert.IsTrue(res[0] == 0x00);
            Assert.IsTrue(res[1] == 0x10);
            Assert.IsTrue(res[2] == 0x55);
            Assert.IsTrue(res[3] == 0x08);
        }

        [TestMethod,Timeout(100)]
        public void TestSkip()
        {
            var b = new BitArray(new bool[] { false, true, false });
            var res = b.Skip(1);

            Assert.IsTrue(res.Length == 2);
        }

        [TestMethod,Timeout(100)]
        public void TestTake()
        {
            var b = new BitArray(new bool[] { false, true, true });
            var res = b.Take(2);
            Assert.IsTrue(res.Length == 2);
        }
    }
}

/*def test_get_bit():
    assert enocean.utils.get_bit(1, 0) == 1
    assert enocean.utils.get_bit(8, 3) == 1
    assert enocean.utils.get_bit(6, 2) == 1
    assert enocean.utils.get_bit(6, 1) == 1


def test_to_hex_string():
    assert enocean.utils.to_hex_string(0) == '00'
    assert enocean.utils.to_hex_string(15) == '0F'
    assert enocean.utils.to_hex_string(16) == '10'
    assert enocean.utils.to_hex_string(22) == '16'

    assert enocean.utils.to_hex_string([0, 15, 16, 22]) == '00:0F:10:16'
    assert enocean.utils.to_hex_string([0x00, 0x0F, 0x10, 0x16]) == '00:0F:10:16'


def test_from_hex_string():
    assert enocean.utils.from_hex_string('00') == 0
    assert enocean.utils.from_hex_string('0F') == 15
    assert enocean.utils.from_hex_string('10') == 16
    assert enocean.utils.from_hex_string('16') == 22

    assert enocean.utils.from_hex_string('00:0F:10:16') == [0, 15, 16, 22]
    assert enocean.utils.from_hex_string('00:0F:10:16') == [0x00, 0x0F, 0x10, 0x16]*/