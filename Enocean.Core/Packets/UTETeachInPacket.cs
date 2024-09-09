using Enocean.Core;
using Enocean.Core.Constants;
using System.Collections;

namespace Enocean.Core.Packets
{
    public class UTETeachInPacket : RadioPacket
    {
        public UTETeachInPacket(PACKET packet_type, List<byte> data, List<byte> optional) : base(packet_type, data, optional)
        {

        }

        public const int TEACH_IN = 0b00;
        public const int DELETE = 0b01;
        public const int NOT_SPECIFIC = 0b10;

        public static List<bool> NOT_ACCEPTED = new() { false, false };
        public static List<bool> TEACHIN_ACCEPTED = new() { false, true };
        public static List<bool> DELETE_ACCEPTED = new() { true, false };
        public static List<bool> EEP_NOT_SUPPORTED = new List<bool>() { true, true };

        public bool Unidirectional { get; set; } = false;
        public bool ResponseExpected { get; set; } = false;
        public int NumberOfChannels { get; set; } = 0xFF;
        public RORG RorgOfEep { get; set; } = RORG.UNDEFINED;
        public int RequestType { get; set; } = NOT_SPECIFIC;
        public int? Channel { get; set; } = null;

        public override bool ContainsEep { get; set; } = true;
        public bool Bidirectional => !Unidirectional;
        public bool TeachIn => RequestType != DELETE;
        public bool Delete => RequestType == DELETE;

        public override Dictionary<string, StateItem> Parse()
        {
            base.Parse();
            Unidirectional = !BitData[^DB6.BIT_7];
            ResponseExpected = !BitData[^DB6.BIT_6];
            RequestType = Utils.FromBitArray(BitData.Skip(BitData.Length - DB6.BIT_5).Take(2));
            m_rorg_manufacturer = Utils.FromBitArray(BitData.Skip(BitData.Length - DB3.BIT_2).Take(3)
                .Concat(BitData.Skip(BitData.Length - DB4.BIT_7).Take(8)));
            Channel = m_data[2];
            m_rorg_type = m_data[5];
            m_rorg_func = m_data[6];
            RorgOfEep = (RORG)m_data[7];

            if (TeachIn)
            {
                Learn = true;
            }
            return m_parsed;
        }
        public RadioPacket CreateResponsePacket(List<byte> sender_id)
        {
            return CreateResponsePacket(sender_id, TEACHIN_ACCEPTED);
        }
        public RadioPacket CreateResponsePacket(List<byte> sender_id, List<bool> response)
        {
            var lst = new List<bool>() { true, false }.Concat(response).Concat(new List<bool>() { false, false, false, true });

            response = response ?? TEACHIN_ACCEPTED;
            List<byte> data = new List<byte> { (byte)(int)m_rorg };

            data.AddRange(new List<byte>() { Utils.FromBitArray(new BitArray(new List<bool>() { true, false }.Concat(response).Concat(new List<bool>() { false, false, false, true }).ToArray())) });
            data.AddRange(m_data.Skip(2).Take(6));
            data.AddRange(sender_id);
            data.AddRange(new List<byte>() { 0x30 }); // status

            List<byte> optional = new List<byte> { 0x03 };
            optional.AddRange(Sender);
            optional.AddRange(new List<byte>() { 0xFF, 0x00 });

            var rp = new RadioPacket(PACKET.RADIO_ERP1, data, optional);
            return rp;
        }
    }
}
