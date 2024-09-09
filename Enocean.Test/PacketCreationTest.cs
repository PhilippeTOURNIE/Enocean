using Enocean.Core.Constants;
using Enocean.Core.EEps;
using Enocean.Core.Packets;

namespace Enocean.Test
{
    [TestClass]
    public class PacketCreationTest
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            var e = new EEP();
        }

        [TestMethod, Timeout(100)]
        public void PacketAssembly()
        {
            var PACKET_CONTENT_1 = new List<byte>(){
        0x55,
        0x00, 0x0A, 0x00, 0x01,
        0x80,
        0xA5, 0x00, 0x00, 0x00, 0x00, 0xDE, 0xAD, 0xBE, 0xEF, 0x00,
        0x18
            };
            var PACKET_CONTENT_2 = new List<byte>(){
        0x55,
        0x00, 0x0A, 0x07, 0x01,
        0xEB,
        0xA5, 0x00, 0x00, 0x00, 0x00, 0xDE, 0xAD, 0xBE, 0xEF, 0x00,
        0x03, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00,
        0xE4
       };
            var PACKET_CONTENT_3 = new List<byte>(){
        0x55,
        0x00, 0x0A, 0x07, 0x01,
        0xEB,
        0xA5, 0x32, 0x20, 0x89, 0x00, 0xDE, 0xAD, 0xBE, 0xEF, 0x00,
        0x03, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00,
        0x43
      };
            var PACKET_CONTENT_4 = new List<byte>(){
        0x55,
        0x00, 0x0A, 0x07, 0x01,
        0xEB,
        0xA5, 0x32, 0x00, 0x00, 0x00, 0xDE, 0xAD, 0xBE, 0xEF, 0x00,
        0x03, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00,
        0x80
       };

            // manually assemble packet
            var packet = new Packet(PACKET.RADIO_ERP1, new List<byte>(), new List<byte>());
            packet.Rorg = RORG.BS4;
            uint senderBytesValue = 0xdeadbeef;
            List<byte> senderBytes = new List<byte>();

            for (int i = 24; i >= 0; i -= 8)
            {
                senderBytes.Add((byte)((senderBytesValue >> i) & 0xff));
            }
            List<byte> data = new List<byte>() { 0, 0, 0, 0 };
            packet.Data.Add((byte)packet.Rorg);
            packet.Data.AddRange(data);
            packet.Data.AddRange(senderBytes);
            packet.Data.Add(0);

            // test content
            var packet_serialized = packet.Build();
            Assert.IsTrue(packet_serialized.SequenceEqual(PACKET_CONTENT_1));


            // set optional data
            var sub_tel_num = 3;
            var destination = new List<byte>() { 255, 255, 255, 255 };   // broadcast
            var dbm = 0xff;
            var security = 0;
            packet.Optional = new List<byte>();
            packet.Optional.Add((byte)sub_tel_num);
            packet.Optional.AddRange(destination);
            packet.Optional.Add((byte)dbm);
            packet.Optional.Add((byte)security);

            //test content
            packet_serialized = packet.Build();
            Assert.IsTrue(packet_serialized.SequenceEqual(PACKET_CONTENT_2));


            // update data based on EEP
            packet.SelectEEP(0x20, 0x01, 1);
            var prop = new Dictionary<string, object>();
            prop["CV"] = 50;
            prop["TMP"] = 21.5;
            prop["ES"] = true;

            packet.SetEEP(prop);

            // test content
            packet_serialized = packet.Build();
            Assert.IsTrue(packet_serialized.SequenceEqual(PACKET_CONTENT_3));
            Assert.IsTrue(packet.RorgFunc == 0x20);
            Assert.IsTrue(packet.RorgType == 0x01);

            // Test the easier method of sending packets.
            packet = Packet.Create(PACKET.RADIO_ERP1, RORG.BS4, 0x20, 0x01, 1, null, null, null, true, prop);
            packet_serialized = packet.Build();
            Assert.IsTrue(packet_serialized.SequenceEqual(PACKET_CONTENT_3));
            Assert.IsTrue(packet.RorgFunc == 0x20);
            Assert.IsTrue(packet.RorgType == 0x01);

            // Test creating RadioPacket directly.
            var prop2 = new Dictionary<string, object>();
            prop2["SP"] = 50;
            packet = RadioPacket.CreateRadio(RORG.BS4, 0x20, 0x01, 2, null, null, null, true, prop2);
            packet_serialized = packet.Build();
            Assert.IsTrue(packet_serialized.SequenceEqual(PACKET_CONTENT_4));
            Assert.IsTrue(packet.RorgFunc == 0x20);
            Assert.IsTrue(packet.RorgType == 0x01);
        }
        [TestMethod, Timeout(100)]
        public void Temperature()
        {
            var TEMPERATURE = new List<byte>(){
                0x55,
                0x00, 0x0A, 0x07, 0x01,
                0xEB,
                0xA5, 0x00, 0x00, 0x55, 0x08, 0x01, 0x81, 0xB7, 0x44, 0x00,
                0x03, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00,
                0x5C
            };

            var prop = new Dictionary<string, object>();
            prop["TMP"] = 26.66666666666666666666666666666666666666666667f;

            var packet = RadioPacket.CreateRadio(RORG.BS4, 0x02, 0x05, null, null, null,
                                new List<byte>() { 0x01, 0x81, 0xB7, 0x44 }, false, prop);
            var packet_serialized = packet.Build();
            Assert.IsTrue(packet_serialized.SequenceEqual(TEMPERATURE));

            Assert.IsTrue((packet as RadioPacket)!.Learn is false);

            TEMPERATURE = new List<byte>(){
                0x55,
                0x00, 0x0A, 0x07, 0x01,
                0xEB,
                0xA5, 0x00, 0x00, 0x55, 0x00, 0x01, 0x81, 0xB7, 0x44, 0x00,
                0x03, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00,
                0xE0
                };

            prop["TMP"] = 26.66666666666666666666666666666666666666666667f;
            packet = RadioPacket.CreateRadio(RORG.BS4, 0x02, 0x05, null, null, null,
                new List<byte>() { 0x01, 0x81, 0xB7, 0x44 }, true, prop);
            packet_serialized = packet.Build();
            Assert.IsTrue(packet_serialized.Count() == TEMPERATURE.Count());
            Assert.IsTrue((packet as RadioPacket)!.Learn is true);
        }
        [TestMethod, Timeout(100)]
        public void MagneticSwitch()
        {
            var MAGNETIC_SWITCH = new List<byte>(){
            0x55,
            0x00, 0x07, 0x07, 0x01,
            0x7A,
            0xD5, 0x08, 0x01, 0x82, 0x5D, 0xAB, 0x00,
            0x03, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00,
            0xBA
            };

            var prop = new Dictionary<string, object>();
            prop["CO"] = "open";
            var packet = RadioPacket.CreateRadio(RORG.BS1, 0x00, 0x01, null, null, null,
            new List<byte>() { 0x01, 0x82, 0x5D, 0xAB }, false, prop);
            var packet_serialized = packet.Build();
            Assert.IsTrue(packet_serialized.SequenceEqual(MAGNETIC_SWITCH));

            Assert.IsTrue((packet as RadioPacket)!.Learn is false);

            MAGNETIC_SWITCH = new List<byte>(){
            0x55,
            0x00, 0x07, 0x07, 0x01,
            0x7A,
            0xD5, 0x00, 0x01, 0x82, 0x5D, 0xAB, 0x00,
            0x03, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00,
            0x06
            };

            packet = RadioPacket.CreateRadio(RORG.BS1, 0x00, 0x01, null, null, null,
            new List<byte>() { 0x01, 0x82, 0x5D, 0xAB }, true, prop);
            packet_serialized = packet.Build();
            Assert.IsTrue(packet_serialized.SequenceEqual(MAGNETIC_SWITCH));

            Assert.IsTrue((packet as RadioPacket)!.Learn is true);

            MAGNETIC_SWITCH = new List<byte>(){
            0x55,
            0x00, 0x07, 0x07, 0x01,
            0x7A,
            0xD5, 0x00, 0x01, 0x82, 0x5D, 0xAB, 0x00,
            0x03, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00,
            0x06 };

            packet = RadioPacket.CreateRadio(RORG.BS1, 0x00, 0x01, null, null, null,
                new List<byte>() { 0x01, 0x82, 0x5D, 0xAB }, true, prop);
            packet_serialized = packet.Build();

            Assert.IsTrue(packet_serialized.SequenceEqual(MAGNETIC_SWITCH));
            Assert.IsTrue((packet as RadioPacket)!.Learn is true);

            MAGNETIC_SWITCH = new List<byte>(){
            0x55,
            0x00, 0x07, 0x07, 0x01,
            0x7A,
            0xD5, 0x09, 0x01, 0x82, 0x5D, 0xAB, 0x00,
            0x03, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00,
            0x2E
            };
            prop["CO"] = "closed";
            packet = RadioPacket.CreateRadio(RORG.BS1, 0x00, 0x01, null, null, null,
                new List<byte>() { 0x01, 0x82, 0x5D, 0xAB }, false, prop);

            packet_serialized = packet.Build();
            Assert.IsTrue(packet_serialized.SequenceEqual(MAGNETIC_SWITCH));
            Assert.IsTrue((packet as RadioPacket)!.Learn is false);

            MAGNETIC_SWITCH = new List<byte>(){
            0x55,
            0x00, 0x07, 0x07, 0x01,
            0x7A,
            0xD5, 0x01, 0x01, 0x82, 0x5D, 0xAB, 0x00,
            0x03, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00,
            0x92
            };

            packet = RadioPacket.CreateRadio(RORG.BS1, 0x00, 0x01, null, null, null,
                new List<byte>() { 0x01, 0x82, 0x5D, 0xAB }, true, prop);
            packet_serialized = packet.Build();
            Assert.IsTrue(packet_serialized.SequenceEqual(MAGNETIC_SWITCH));
            Assert.IsTrue((packet as RadioPacket)!.Learn is true);
        }

        [TestMethod, Timeout(100)]
        public void Switch()
        {
            var SWITCH = new List<byte>(){
                0x55,
                0x00, 0x07, 0x07, 0x01,
                0x7A,
                0xF6, 0x50, 0x00, 0x29, 0x89, 0x79, 0x30,
                0x03, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00,
                0x61
            };

            // test also enum setting by integer value with EB0
            var prop = new Dictionary<string, object>();
            prop["SA"] = "No 2nd action";
            prop["EB"] = 1;
            prop["R1"] = "Button BI";
            prop["T21"] = true;
            prop["NU"] = true;
            var packet = RadioPacket.CreateRadio(RORG.RPS, 0x02, 0x02, null, null, null,
                new List<byte>() { 0x00, 0x29, 0x89, 0x79 }, false, prop);

            var packet_serialized = packet.Build();
            Assert.IsTrue(packet_serialized.SequenceEqual(SWITCH));


            SWITCH = new List<byte>(){
            0x55,
            0x00, 0x07, 0x07, 0x01,
            0x7A,
            0xF6, 0x00, 0x00, 0x29, 0x89, 0x79, 0x20,
            0x03, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00,
            0xD2
            };
            var prop2 = new Dictionary<string, object>();
            prop2["SA"] = "No 2nd action";
            prop2["EB"] = "released";
            prop2["T21"] = true;
            prop2["NU"] = false;
            packet = RadioPacket.CreateRadio(RORG.RPS, 0x02, 0x02, null, null, null,
                new List<byte>() { 0x00, 0x29, 0x89, 0x79 }, false, prop2);
            packet_serialized = packet.Build();
            Assert.IsTrue(packet_serialized.SequenceEqual(SWITCH));
        }

        [TestMethod, Timeout(100)]
        public void IllegalEepEnum1()
        {
            var prop = new Dictionary<string, object>();
            prop["EB"] = "inexisting";
            try
            {
                RadioPacket.CreateRadio(RORG.RPS, 0x02, 0x02, null, null, null,
                    new List<byte>() { 0x00, 0x29, 0x89, 0x79 }, false, prop);
                Assert.Fail("Exception should be launch");
            }
            catch
            {
            }
        }

        [TestMethod, Timeout(100)]
        public void IllegalEepEnum2()
        {
            var prop = new Dictionary<string, object>();
            prop["EB"] = "2";
            try
            {
                RadioPacket.CreateRadio(RORG.RPS, 0x02, 0x02, null, null, null, new List<byte>() { 0x00, 0x29, 0x89, 0x79 }, false, prop);
                Assert.Fail("Exception should be launch");
            }
            catch
            {
            }
        }

        [TestMethod, Timeout(100)]
        public void test_packets_with_destination()
        {
            var prop = new Dictionary<string, object>();
            prop["TMP"] = 26.66666666666666666666666666666666666666666667f;
            var TEMPERATURE = new List<byte>(){
                0x55,
                0x00, 0x0A, 0x07, 0x01,
                0xEB,
                0xA5, 0x00, 0x00, 0x55, 0x08, 0x01, 0x81, 0xB7, 0x44, 0x00,
                0x03, 0xDE, 0xAD, 0xBE, 0xEF, 0xFF, 0x00,
                0x5F
            };

            var packet = RadioPacket.CreateRadio(RORG.BS4, 0x02, 0x05, null, null,
                new List<byte>() { 0xDE, 0xAD, 0xBE, 0xEF },
                new List<byte>() { 0x01, 0x81, 0xB7, 0x44 },
                false, prop);

            var packet_serialized = packet.Build();
            Assert.IsTrue(packet_serialized.Count() == TEMPERATURE.Count());
            Assert.IsTrue(packet_serialized.SequenceEqual(TEMPERATURE));
            Assert.IsTrue((packet as RadioPacket)!.Learn is false);
            Assert.IsTrue((packet as RadioPacket)!.SenderToUint == 25278276);
            Assert.IsTrue((packet as RadioPacket)!.DestinationToUint == 3735928559);

            var MAGNETIC_SWITCH = new List<byte>(){
                0x55,
                0x00, 0x07, 0x07, 0x01,
                0x7A,
                0xD5, 0x08, 0x01, 0x82, 0x5D, 0xAB, 0x00,
                0x03, 0xDE, 0xAD, 0xBE, 0xEF, 0xFF, 0x00,
                0xB9
            };

            var prop2 = new Dictionary<string, object>();
            prop2["CO"] = "open";
            packet = RadioPacket.CreateRadio(RORG.BS1, 0x00, 0x01, null, null,
                new List<byte>() { 0xDE, 0xAD, 0xBE, 0xEF },
                new List<byte>() { 0x01, 0x82, 0x5D, 0xAB }, false, prop2);

            packet_serialized = packet.Build();
            Assert.IsTrue(packet_serialized.SequenceEqual(MAGNETIC_SWITCH));
            Assert.IsTrue((packet as RadioPacket)!.Learn is false);
        }

        [TestMethod, Timeout(100)]
        public void Vld()
        {
            var SWITCH = new List<byte>(){
            0x55,
            0x00, 0x09, 0x07, 0x01,
            0x56,
            0xD2, 0x01, 0x1E, 0x64, 0xDE, 0xAD, 0xBE, 0xEF, 0x00,
            0x03, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00,
            0x5A
            };

            var prop = new Dictionary<string, object>();
            prop["DV"] = 0;
            prop["IO"] = 0x1E;
            prop["OV"] = 0x64;

            var packet = RadioPacket.CreateRadio(RORG.VLD, 0x01, 0x01, null, 1, null, null, false, prop);
            var packet_serialized = packet.Build();
            Assert.IsTrue(packet_serialized.SequenceEqual(SWITCH));
            Assert.IsTrue(packet.Parsed["CMD"].raw_value == (0x01).ToString());
            Assert.IsTrue(packet.Parsed["IO"].raw_value == (0x1E).ToString());
            Assert.IsTrue(packet.Parsed["IO"].value == "All output channels supported by the device");
            Assert.IsTrue(packet.Parsed["DV"].value == "Switch to new output value");
            Assert.IsTrue(packet.Parsed["OV"].value == "Output value 100% or ON");
        }

        [TestMethod, Timeout(100)]
        public void fails()
        {
            try
            {
                Packet.Create(PACKET.RESPONSE, (RORG)0xA5, 0x01, 0x01);
                Assert.Fail("Exception should be launch");
            }
            catch (Exception)
            {
            }

            try
            {
                Packet.Create(PACKET.RADIO_ERP1, (RORG)0xA6, 0x01, 0x01);
                Assert.Fail("Exception should be launch");
            }
            catch (Exception)
            {
            }
        }
    }
}
