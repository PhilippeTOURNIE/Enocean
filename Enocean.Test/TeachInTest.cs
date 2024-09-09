using Enocean.Core;
using Enocean.Core.Constants;
using Enocean.Core.EEps;
using Enocean.Core.Packets;

namespace Enocean.Test
{
    [TestClass]
    public class TeachInTest
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            var e = new EEP();
        }

        [TestMethod,Timeout(100)]
        public void ute_in()
        {
            var communicator_base_id = new List<byte> { 0xDE, 0xAD, 0xBE, 0xEF };

            var (status, buf, packet) = Packet.ParseMsg(
                    new List<byte> {
                    0x55,
                    0x00, 0x0D, 0x07, 0x01,
                    0xFD,
                    0xD4, 0xA0, 0xFF, 0x3E, 0x00, 0x01, 0x01, 0xD2, 0x01, 0x94, 0xE3, 0xB9, 0x00,
                    0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x40, 0x00,
                    0xAB }
                    );

            Assert.IsTrue(((UTETeachInPacket)packet!).SenderToHex == "01:94:E3:B9");
            Assert.IsTrue(((UTETeachInPacket)packet!).Unidirectional is false);
            Assert.IsTrue(((UTETeachInPacket)packet!).Bidirectional is true);
            Assert.IsTrue(((UTETeachInPacket)packet!).ResponseExpected is true);
            Assert.IsTrue(((UTETeachInPacket)packet!).NumberOfChannels == 0xFF);
            Assert.IsTrue(((UTETeachInPacket)packet!).RorgManufacturer == 0x3E);
            Assert.IsTrue(((UTETeachInPacket)packet!).RorgOfEep == RORG.VLD);
            Assert.IsTrue(((UTETeachInPacket)packet!).RorgFunc == 0x01);
            Assert.IsTrue(((UTETeachInPacket)packet!).RorgType == 0x01);
            Assert.IsTrue(((UTETeachInPacket)packet!).TeachIn is true);
            Assert.IsTrue(((UTETeachInPacket)packet!).Delete is false);
            Assert.IsTrue(((UTETeachInPacket)packet!).Learn is true);
            Assert.IsTrue(((UTETeachInPacket)packet!).ContainsEep is true);

            var response_packet = ((UTETeachInPacket)packet!).CreateResponsePacket(communicator_base_id);
            Assert.IsTrue(response_packet.SenderToHex == "DE:AD:BE:EF");
            Assert.IsTrue(response_packet.DestinationToHex == "01:94:E3:B9");

            Assert.IsTrue(response_packet.BitData.Skip(response_packet.BitData.Length - DB6.BIT_5).Take(2)[0]
                == UTETeachInPacket.TEACHIN_ACCEPTED[0]);
            Assert.IsTrue(response_packet.BitData.Skip(response_packet.BitData.Length - DB6.BIT_5).Take(2)[1]
                == UTETeachInPacket.TEACHIN_ACCEPTED[1]);

            var t1 = response_packet.Data[2..7];
            var t2 = packet.Data[2..7];
            for (int i = 0; i < t1.Count; i++)
            {
                Assert.IsTrue(t1[i] == t2[i]);
            }
        }

        [TestMethod(),Timeout(100)]
        public void ute_in_2()
        {
            var communicator_base_id = new List<byte> { 0x05,0xA1, 0xDB, 0xFC };

            var (status, buf, packet) = Packet.ParseMsg(
                    new List<byte> {
                    0x55,
                    0x00, 0x0D, 0x07, 0x01,
                    0xFD,
                    0xD4,
                    0xA0,0x01,0x46,0x00,0x00,0x05,0xD2,
                    0x05,0x91,0x62,0xC6,
                    0x00,
                    0x00,
                    0xFF,0xFF,0xFF,0xFF,
                    0x30,
                    0x00,
                    0x6C }
                    );

            var response_packet = ((UTETeachInPacket)packet!).CreateResponsePacket(communicator_base_id);
            Assert.IsTrue(response_packet.SenderToHex == "05:A1:DB:FC");
            Assert.IsTrue(response_packet.DestinationToHex == "05:91:62:C6");
            Assert.IsTrue(response_packet.Status == 0x30);

        }
    }
}