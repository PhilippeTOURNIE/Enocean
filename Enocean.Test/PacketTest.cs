using Enocean.Core;
using Enocean.Core.Constants;
using Enocean.Core.EEps;
using Enocean.Core.Packets;
using System.Diagnostics;
using static Enocean.Core.Constants.PACKET;
namespace Enocean.Test
{
    [TestClass]
    public class PacketTest
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            var e = new EEP();
        }

        public record TestTelegram
        {
            public Enocean.Core.Constants.PACKET PacketType;
            public List<byte> Msg = new List<byte>();
            public int data_len;
            public int opt_len;
        }
        [TestMethod,Timeout(100)]
        public void PacketExamples()
        {
            var telegramms = new List<TestTelegram>()
            {
                new TestTelegram()
                {
                   // Radio VLD
                   PacketType=RADIO_ERP1,
                    Msg=new List<byte>()
                    {
                    0x55,
                    0x00, 0x0F, 0x07, 0x01,
                    0x2B,
                    0xD2, 0xDD, 0xDD, 0xDD, 0xDD, 0xDD, 0xDD, 0xDD, 0xDD, 0xDD, 0x00, 0x80, 0x35, 0xC4, 0x00,
                    0x03, 0xFF, 0xFF, 0xFF, 0xFF, 0x4D, 0x00,
                    0x36
                    },
                    data_len=15,
                    opt_len =7
                },
               
                 // CO_WR_SLEEP
                 new TestTelegram()
                {
                    PacketType= COMMON_COMMAND,
                    Msg =new List<byte>()
                    {
                        0x55,
                        0x00, 0x05, 0x00, 0x05,
                        0xDB,
                        0x01, 0x00, 0x00, 0x00, 0x0A,
                        0x54 },
                    data_len = 5,
                    opt_len = 0,
                },
                // CO_WR_RESET
                new TestTelegram()
                {
                    PacketType= COMMON_COMMAND,
                    Msg= new List<byte>()
                    {
                        0x55,
                        0x00, 0x01, 0x00, 0x05,
                        0x70,
                        0x02,
                        0x0E
                    },
                    data_len = 1,
                    opt_len = 0,
                },
            // CO_RD_IDBASE
              new TestTelegram()
              {
                PacketType= COMMON_COMMAND,
                Msg=new List<byte>(){
                    0x55,
                    0x00, 0x01, 0x00, 0x05,
                    0x70,
                    0x08,
                    0x38 },
                data_len = 1,
                opt_len = 0,
            },
            // Response RET_OK
              new TestTelegram()
              {
                PacketType = RESPONSE,
                Msg=new List<byte>(){
                    0x55,
                    0x00, 0x05, 0x00, 0x02,
                    0xCE,
                    0x00, 0xFF, 0x80, 0x00, 0x00,
                    0xDA },
                data_len = 5,
                opt_len = 0,
        },
        // REMOTE_MAN_COMMAND
              new TestTelegram()
              {
                PacketType = REMOTE_MAN_COMMAND,
                Msg=new List<byte>(){
                    0x55,
                    0x00, 0x19, 0x00, 0x07, 0x8D,
                    0x12, 0x12, 0x07, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00,
                    0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
                    0xDA },
                data_len = 25,
                opt_len = 0,
        },
        // QueryID
              new TestTelegram()
              {
                  PacketType = REMOTE_MAN_COMMAND,
            Msg=new List<byte>(){
                0x55,
                0x00, 0x0C, 0x00, 0x07,
                0xEF,
                0x00, 0x04, 0x07, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00,
                0x65 },
            data_len = 12,
            opt_len = 0,
        },
        // Custom test, containing 0x55 in message
              new TestTelegram()
              {
                  PacketType = RESPONSE,
            Msg=new List<byte>(){
                0x55,
                0x00, 0x05, 0x01, 0x02,
                0xDB,
                0x00, 0xFF, 0x9E, 0x55, 0x00,
                0x0A,
                0x79,
                // unnecessary data, to check for message length checking
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
            },
            data_len = 5,
            opt_len = 1,
            }
            };

            foreach (var item in telegramms)
            {
                var (status, remainder, pack) = Packet.ParseMsg(item.Msg);
                Assert.IsTrue(status == PARSE_RESULT.OK);
                Assert.IsTrue(pack!.PacketType != 0x00);
                Assert.IsTrue(pack!.PacketType == item.PacketType);
                Assert.IsTrue(pack!.Data.Count() == item.data_len);
                Assert.IsTrue(pack!.Optional.Count() == item.opt_len);
                Assert.IsTrue(pack!.Status == 0x00);
                Assert.IsTrue(pack!.RepeaterCount == 0);
            }
        }

        [TestMethod,Timeout(100)]
        public void PacketFails()
        {
            // Tests designed to fail.
            //These include changes to checksum, data length or something like that.
            var fail_examples = new List<byte[]>(){
             new byte[]{
                0x55,
                0x00, 0x0F, 0x07, 0x01,
                0x2B,
                0xD2, 0xDD, 0xDC, 0xDD, 0xDD, 0xDD, 0xDD, 0xDD, 0xDD, 0xDD, 0x00, 0x80, 0x35, 0xC4, 0x00,
                0x03, 0xFF, 0xFF, 0xFF, 0xFF, 0x4D, 0x00,
                0x36
            },
             new byte[]{
                0x55,
                0x00, 0x0F, 0x07, 0x01,
                0x2B,
                0xD2, 0xDD, 0xDD, 0xDD, 0xDD, 0xDD, 0xDD, 0xDD, 0xDD, 0xDD, 0x00, 0x80, 0x35, 0xC4, 0x00,
                0x03, 0xFF, 0xFF, 0xFF, 0xFF, 0x4D, 0x00,
                0x37
            },
             new byte[]{
                0x55,
                0x00, 0x0F, 0x07, 0x01,
                0x1B,
                0xD2, 0xDD, 0xDD, 0xDD, 0xDD, 0xDD, 0xDD, 0xDD, 0xDD, 0xDD, 0x00, 0x80, 0x35, 0xC4, 0x00,
                0x03, 0xFF, 0xFF, 0xFF, 0xFF, 0x4D, 0x00,
                0x36
            },
             new byte[]{
                0x55,
                0x00, 0x01, 0x00, 0x05,
                0x70,
                0x38
            },
             new byte[]{
                 0x55,
                 0x00, 0x01
             }
                };
            foreach (var msg in fail_examples)
            {
                Debug.Print(msg[msg.Length - 1].ToString());
                var (status, remainder, packet) = Packet.ParseMsg(msg.ToList());
                Assert.IsTrue(new PARSE_RESULT[] { PARSE_RESULT.INCOMPLETE, PARSE_RESULT.CRC_MISMATCH }.Contains(status));
            }

        }

        [TestMethod,Timeout(100)]
        public void PacketEquals()
        {
            var data_1 = new List<byte>(){
            0x55,
            0x00, 0x01, 0x00, 0x05,
            0x70,
            0x08,
            0x38
                 };
            var data_2 = new List<byte>(){
            0x55,
            0x00, 0x01, 0x00, 0x05,
            0x70,
            0x08,
            0x38
           };

            var (_, _, packet_1) = Packet.ParseMsg(data_1);
            var (_, _, packet_2) = Packet.ParseMsg(data_2);

            Assert.IsTrue(packet_1!.ToString() == packet_2!.ToString());
        }
        [TestMethod,Timeout(100)]
        public void PacketEvent()
        {
            var data = new List<byte>() {
                0x55,
                0x00, 0x01, 0x00, 0x04,
                0x77,
                0x01,
                0x07 };

            var (_, _, packet) = Packet.ParseMsg(data);
            Assert.IsTrue(packet is EventPacket);
            Assert.IsTrue(((EventPacket)packet).Event ==(int) EVENT_CODE.SA_RECLAIM_NOT_SUCCESFUL);
            Assert.IsTrue(((EventPacket)packet).EventData.Count() == 0);
            Assert.IsTrue(packet.Optional.Count() == 0);
        }
    }
}
