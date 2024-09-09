using Enocean.Core.Communicators;
using Enocean.Core.Constants;
using Enocean.Core.EEps;
using Enocean.Core.Packets;

namespace Enocean.Test
{
    public class CommunicatorForTest : Communicator
    {
        public CommunicatorForTest(Action<Packet>? callback = null) : base(callback)
        {

        }
    }

    [TestClass]
    public class CommunicatorTest
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            var e = new EEP();
        }

        [TestMethod, Timeout(100)]
        public void Buffer()
        {
            // Test buffer parsing for Communicator 
            var data = new List<byte>(){
            0x55,
            0x00, 0x0A, 0x07, 0x01,
            0xEB,
            0xA5, 0x00, 0x00, 0x55, 0x08, 0x01, 0x81, 0xB7, 0x44, 0x00,
            0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x2D, 0x00,
            0x75
            };

            var com = new CommunicatorForTest();
            com.Buffer.AddRange(data.Take(5));
            com.Parse();
            Assert.IsTrue(com.Receive.Count() == 0);
            com.Buffer.AddRange(data.Skip(5));
            com.Parse();
            Assert.IsTrue(com.Receive.Count() == 1);
        }

        [TestMethod, Timeout(100)]
        public void Send()
        {
            /// Test sending packets to Communicator
            var com = new CommunicatorForTest();
            Assert.IsTrue(com.Send(new RadioPacket(PACKET.COMMON_COMMAND, [0x08])) is true);
            Assert.IsTrue(com.Transmit.Count == 1);
            Assert.IsTrue(com.GetFromSendQueue() is Packet);
        }

        [TestMethod, Timeout(100)]
        public void Stop()
        {
            var com = new CommunicatorForTest();
            com.Stop();
            Assert.IsTrue(com.StopFlag);
        }

        public void callback(Packet a)
        {
            if (a.PacketType == PACKET.RADIO)
                return;
            return;
        }

        [TestMethod, Timeout(100)]
        public void CallBack()
        {
            var data = new List<byte>(){
            0x55,
            0x00, 0x0A, 0x07, 0x01,
            0xEB,
            0xA5, 0x00, 0x00, 0x55, 0x08, 0x01, 0x81, 0xB7, 0x44, 0x00,
            0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x2D, 0x00,
            0x75
            };

            var com = new CommunicatorForTest(callback);
            com.Buffer = data;
            com.Parse();
            Assert.IsTrue(com.Receive.Count == 0);

        }

        [TestMethod, Timeout(10000)]
        public void BaseId()
        {
            var com = new CommunicatorForTest();
            Assert.IsTrue(com.BaseId == null);

            var other_data = new List<byte>(){
            0x55,
            0x00, 0x0A, 0x07, 0x01,
            0xEB,
            0xA5, 0x00, 0x00, 0x55, 0x08, 0x01, 0x81, 0xB7, 0x44, 0x00,
            0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x2D, 0x00,
            0x75
            };
            var response_data = new List<byte>()
            {
            0x55,
            0x00, 0x05, 0x00, 0x02,
            0xCE,
            0x00, 0xFF, 0x87, 0xCA, 0x00,
            0xA3
            };

            com.Buffer.AddRange(other_data);
            com.Buffer.AddRange(response_data);
            com.Parse();
            var baseID = new List<byte> { 0xFF, 0x87, 0xCA, 0x00 };
            Assert.IsTrue(com.BaseId.SequenceEqual(baseID));

            Assert.IsTrue(com.Receive.Count() == 2);
        }
    }
}
