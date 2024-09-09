using Enocean.Core.Constants;
using Enocean.Core.EEps;
using Enocean.Core.Packets;

namespace Enocean.Test
{
    [TestClass]
    public class EEPTest
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            var e = new EEP();
        }

        /// <summary>
        /// Tests RADIO message for EEP -profile 0xA5 0x02 0x05 '''
        /// 
        /// </summary>
        [TestMethod, Timeout(100)]
        public void TestTemperature()
        {
            (PARSE_RESULT res, List<byte> res1, Packet? packet) = Packet.ParseMsg(new List<byte>()
            {
                0x55,
                0x00, 0x0A, 0x07, 0x01,
                0xEB,
                0xA5, 0x00, 0x00, 0x55, 0x08, 0x01, 0x81, 0xB7, 0x44, 0x00,
                0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x2D, 0x00,
                0x75
            });

            var parse = packet!.ParseEEP(0x02, 0x05);
            Assert.IsTrue(parse.First().Key == "TMP");
            Assert.IsTrue(((RadioPacket)packet).Learn == false);
            Assert.IsTrue(parse.First().Value.raw_value == "85");
            Assert.IsTrue(Math.Round(double.Parse(parse.First().Value.value!), 1) == 26.7);
            Assert.IsTrue(packet.Rorg == RORG.BS4);
            Assert.IsTrue(packet.RorgFunc == 0x02);
            Assert.IsTrue(packet.RorgType == 0x05);
            Assert.IsTrue(((RadioPacket)packet).ContainsEep == false);
            Assert.IsTrue(packet.Status == 0x00);
            Assert.IsTrue(packet.RepeaterCount == 0);
            Assert.IsTrue(((RadioPacket)packet).Sender[0] == 0x01);
            Assert.IsTrue(((RadioPacket)packet).Sender[1] == 0x81);
            Assert.IsTrue(((RadioPacket)packet).Sender[2] == 0xB7);
            Assert.IsTrue(((RadioPacket)packet).Sender[3] == 0x44);
            Assert.IsTrue(((RadioPacket)packet).SenderToHex == "01:81:B7:44");
        }

        /// <summary>
        /// Tests RADIO message for EEP -profile 0xD5 0x00 0x01
        /// </summary>
        [TestMethod, Timeout(100)]
        public void MagneticSwitchOpen()
        {
            //open
            (PARSE_RESULT res, List<byte> res1, Packet? packet) = Packet.ParseMsg(new List<byte>()
            {   0x55,
                0x00, 0x07, 0x07, 0x01,
                0x7A,
                0xD5, 0x08, 0x01, 0x82, 0x5D, 0xAB, 0x00,
                0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x36, 0x00,
                0x53
            });

            var parse = packet!.ParseEEP(0x00, 0x01);
            Assert.IsTrue(parse.First().Key == "CO");
            Assert.IsTrue(((RadioPacket)packet).Learn == false);
            Assert.IsTrue(parse.First().Value.raw_value == "0");
            Assert.IsTrue(parse.First().Value.value == "open");
            Assert.IsTrue(packet.Status == 0x00);
            Assert.IsTrue(packet.RepeaterCount == 0);
        }

        [TestMethod, Timeout(100)]
        public void MagneticSwitchClose()
        {
            //close
            (PARSE_RESULT res, List<byte> res1, Packet? packet) = Packet.ParseMsg(new List<byte>()
            {   0x55,
                0x00, 0x07, 0x07, 0x01,
                0x7A,
                0xD5, 0x09, 0x01, 0x82, 0x5D, 0xAB, 0x00,
                0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x36, 0x00,
                0xC7
            });

            var parse = packet!.ParseEEP(0x00, 0x01);
            Assert.IsTrue(parse.First().Key == "CO");
            Assert.IsTrue(((RadioPacket)packet).Learn == false);
            Assert.IsTrue(parse.First().Value.raw_value == "1");
            Assert.IsTrue(parse.First().Value.value == "closed");
            Assert.IsTrue(packet.Status == 0x00);
            Assert.IsTrue(packet.RepeaterCount == 0);
        }

        [TestMethod, Timeout(100)]
        public void SwitchPressed()
        {
            (PARSE_RESULT res, List<byte> res1, Packet? packet) = Packet.ParseMsg(new List<byte>()
            {
                0x55,
                0x00, 0x07, 0x07, 0x01,
                0x7A,
                0xF6, 0x50, 0x00, 0x29, 0x89, 0x79, 0x30,
                0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x37, 0x00,
                0x9D
            });

            var parse = packet!.ParseEEP(0x02, 0x02);
            var lst = new List<string>() { "R1", "EB", "R2", "SA", "T21", "NU" };
            Assert.IsTrue(parse.Any(x => lst.Contains(x.Key)));
            Assert.IsTrue(parse["SA"].value == "No 2nd action");
            Assert.IsTrue(parse["EB"].value == "pressed");
            Assert.IsTrue(parse["R1"].value == "Button BI");
            Assert.IsTrue(parse["T21"].value == "1");
            Assert.IsTrue(parse["NU"].value == "1");
            Assert.IsTrue(((RadioPacket)packet).Learn == true);
            Assert.IsTrue(packet.Status == 0x30);
            Assert.IsTrue(packet.RepeaterCount == 0);
        }

        [TestMethod, Timeout(100)]
        public void SwitchRelease()
        {
            (PARSE_RESULT res, List<byte> res1, Packet? packet) = Packet.ParseMsg(new List<byte>()
            {
                0x55,
                0x00, 0x07, 0x07, 0x01,
                0x7A,
                0xF6, 0x00, 0x00, 0x29, 0x89, 0x79, 0x20,
                0x02, 0xFF, 0xFF, 0xFF, 0xFF, 0x4A, 0x00,
                0x03
            });

            var parse = packet!.ParseEEP(0x02, 0x02);
            var lst = new List<string>() { "R1", "EB", "R2", "SA", "T21", "NU" };
            Assert.IsTrue(parse.Any(x => lst.Contains(x.Key)));
            Assert.IsTrue(parse["SA"].value == "No 2nd action");
            Assert.IsTrue(parse["EB"].value == "released");
            Assert.IsTrue(parse["T21"].value == "1");
            Assert.IsTrue(parse["NU"].value == "0");
            Assert.IsTrue(((RadioPacket)packet).Learn == true);
            Assert.IsTrue(packet.Status == 0x20);
            Assert.IsTrue(packet.RepeaterCount == 0);
        }

        [TestMethod, Timeout(100)]
        public void EepParsing()
        {
            (PARSE_RESULT res, List<byte> res1, Packet? packet) = Packet.ParseMsg(new List<byte>()
            {
                0x55,
                0x00, 0x0A, 0x07, 0x01,
                0xEB,
                0xA5, 0x08, 0x28, 0x46, 0x80, 0x01, 0x8A, 0x7B, 0x30, 0x00,
                0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x49, 0x00,
                0x26
            });

            Assert.IsTrue(packet?.RorgFunc == 0x02);
            Assert.IsTrue(packet?.RorgType == 0x05);
            Assert.IsTrue(((RadioPacket)packet)?.ContainsEep == true);
            Assert.IsTrue(((RadioPacket)packet)?.Learn == true);
            Assert.IsTrue(packet?.Status == 0x00);
            Assert.IsTrue(packet?.RepeaterCount == 0);
        }

        /// <summary>
        ///  Magnetic switch -example
        /// </summary>
        [TestMethod, Timeout(100)]
        public void EepRemaining()
        {
            (PARSE_RESULT res, List<byte> res1, Packet? packet) = Packet.ParseMsg(new List<byte>()
            {
                0x55,
                0x00, 0x07, 0x07, 0x01,
                0x7A,
                0xD5, 0x08, 0x01, 0x82, 0x5D, 0xAB, 0x00,
                0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x36, 0x00,
                0x53
            });

            var parse = packet!.ParseEEP(0x00, 0x01);
            Assert.IsTrue(parse.First().Key == "CO");
            // Temperature-example
            (PARSE_RESULT res_2, List<byte> res1_2, Packet? packet_2) = Packet.ParseMsg(new List<byte>()
            {
                0x55,
                0x00, 0x0A, 0x07, 0x01,
                0xEB,
                0xA5, 0x00, 0x00, 0x55, 0x08, 0x01, 0x81, 0xB7, 0x44, 0x00,
                0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x2D, 0x00,
                0x75
            });
            // If this fails, the data is retained from the last Packet parsing!
            var parse2 = packet_2!.ParseEEP(0x00, 0x01);
            Assert.IsTrue(!parse2!.Any());
            // Once we have parse with the correct func and type, this should pass.
            var parse3 = packet_2!.ParseEEP(0x02, 0x05);
            Assert.IsTrue(parse3!.First().Key == "TMP");
        }
        [TestMethod, Timeout(100)]
        public void Direction()
        {
            (PARSE_RESULT res, List<byte> res1, Packet? packet) = Packet.ParseMsg(new List<byte>()
            {
                0x55,
                0x00, 0x0A, 0x07, 0x01,
                0xEB,
                0xA5, 0x32, 0x20, 0x89, 0x00, 0xDE, 0xAD, 0xBE, 0xEF, 0x00,
                0x03, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00,
                0x43
            });

            var parse = packet!.ParseEEP(0x20, 0x01, 1);
            var lst = new List<string>() { "CV", "SO", "ENIE", "ES", "BCAP", "CCO", "FTS", "DWO", "ACO", "TMP" };
            Assert.IsTrue(parse.Any(x => lst.Contains(x.Key)));
            Assert.IsTrue(parse["CV"].value == "50");
            var parse2 = packet!.ParseEEP(0x20, 0x01, 2);
            var lst2 = new List<string>() { "SP", "TMP", "RIN", "LFS", "VO", "VC", "SB", "SPS", "SPN", "RCU" };
            Assert.IsTrue(parse.Any(x => lst2.Contains(x.Key)));
            Assert.IsTrue(parse2["SP"].value == "50");
        }

        [TestMethod, Timeout(100)]
        public void Vld()
        {
            (PARSE_RESULT res, List<byte> res1, Packet? packet) = Packet.ParseMsg(new List<byte>()
            {
                0x55,
                0x00, 0x09, 0x07, 0x01,
                0x56,
                0xD2, 0x04, 0x00, 0x64, 0x01, 0x94, 0xE3, 0xB9, 0x00,
                0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x40, 0x00,
                0xE4
            });

            var parse = packet!.ParseEEP(0x01, 0x01);
            Assert.IsTrue(packet.Rorg == RORG.VLD);
            var lst = new List<string>() { "PF", "PFD", "CMD", "OC", "EL", "IO", "LC", "OV" };
            Assert.IsTrue(parse.Any(x => lst.Contains(x.Key)));

            Assert.IsTrue(parse["EL"].raw_value == "0");
            Assert.IsTrue(parse["EL"].value == "Error level 0: hardware OK");
            Assert.IsTrue(parse["PF"].raw_value == "0");
            Assert.IsTrue(parse["PF"].value == "Power Failure Detection disabled/not supported");
            Assert.IsTrue(parse["PFD"].raw_value == "0");
            Assert.IsTrue(parse["PFD"].value == "Power Failure Detection not detected/not supported/disabled");
            Assert.IsTrue(parse["IO"].raw_value == "0");
            Assert.IsTrue(parse["IO"].value == "Output channel 0 (to load)");
            Assert.IsTrue(parse["OV"].raw_value == "100");
            Assert.IsTrue(parse["OV"].value == "Output value 100% or ON");
            Assert.IsTrue(parse["OC"].raw_value == "0");
            Assert.IsTrue(parse["OC"].value == "Over current switch off: ready / not supported");
            Assert.IsTrue(parse["LC"].raw_value == "0");
            Assert.IsTrue(parse["LC"].value == "Local control disabled / not supported");

        }
        [TestMethod, Timeout(100)]
        public void Vld2()
        {
            (PARSE_RESULT res, List<byte> res1, Packet? packet) = Packet.ParseMsg(new List<byte>()
            {
                0x55,
                0x00, 0x09, 0x07, 0x01,
                0x56,
                0xD2, 0x04, 0x00, 0x00, 0x01, 0x94, 0xE3, 0xB9, 0x00,
                0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x40, 0x00,
                0xBF
            });

            var parse = packet!.ParseEEP(0x01, 0x01);
            Assert.IsTrue(packet.Rorg == RORG.VLD);
            var lst = new List<string>() { "PF", "PFD", "CMD", "OC", "EL", "IO", "LC", "OV" };
            Assert.IsTrue(parse.Any(x => lst.Contains(x.Key)));

            Assert.IsTrue(parse["EL"].raw_value == "0");
            Assert.IsTrue(parse["EL"].value == "Error level 0: hardware OK");
            Assert.IsTrue(parse["PF"].raw_value == "0");
            Assert.IsTrue(parse["PF"].value == "Power Failure Detection disabled/not supported");
            Assert.IsTrue(parse["PFD"].raw_value == "0");
            Assert.IsTrue(parse["PFD"].value == "Power Failure Detection not detected/not supported/disabled");
            Assert.IsTrue(parse["IO"].raw_value == "0");
            Assert.IsTrue(parse["IO"].value == "Output channel 0 (to load)");
            Assert.IsTrue(parse["OV"].raw_value == "0");
            Assert.IsTrue(parse["OV"].value == "Output value 0% or OFF");
            Assert.IsTrue(parse["OC"].raw_value == "0");
            Assert.IsTrue(parse["OC"].value == "Over current switch off: ready / not supported");
            Assert.IsTrue(parse["LC"].raw_value == "0");
            Assert.IsTrue(parse["LC"].value == "Local control disabled / not supported");
        }
        [TestMethod, Timeout(100)]
        public void Fails()
        {
            (PARSE_RESULT res, List<byte> res1, Packet? packet) = Packet.ParseMsg(new List<byte>()
            {
                0x55,
                0x00, 0x07, 0x07, 0x01,
                0x7A,
                0xD5, 0x08, 0x01, 0x82, 0x5D, 0xAB, 0x00,
                0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x36, 0x00,
                0x53
            });

            var eep = new EEP();
            // Mock initialization failure

            var profile = eep.FindProfil(packet!.BitData, (RORG)0xD5, 0x00, 0x01);

            var v = new Dictionary<string, object>();
            v["ASD"] = 1;
            Assert.IsTrue(eep.SetValues(profile!, packet.BitData, packet.BitStatus, v).Item1 != null);

            Assert.IsTrue(eep.FindProfil(packet!.BitData, (RORG)0xFF, 0x00, 0x01) == null);
            Assert.IsTrue(eep.FindProfil(packet!.BitData, (RORG)0xD5, 0xFF, 0x01) == null);
            Assert.IsTrue(eep.FindProfil(packet!.BitData, (RORG)0xD5, 0x00, 0xFF) == null);

            (PARSE_RESULT res_2, List<byte> res1_2, Packet? packet_2) = Packet.ParseMsg(new List<byte>()
            {
                0x55,
                0x00, 0x09, 0x07, 0x01,
                0x56,
                0xD2, 0x04, 0x00, 0x00, 0x01, 0x94, 0xE3, 0xB9, 0x00,
                0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x40, 0x00,
                0xBF
            });

            Assert.IsTrue(eep.FindProfil(packet_2!.BitData, (RORG)0xD2, 0x01, 0x01)?.Any());
            Assert.IsTrue(eep.FindProfil(packet_2!.BitData, (RORG)0xD2, 0x01, 0x01, null, -1)?.Count() == 0);
        }
    }
}