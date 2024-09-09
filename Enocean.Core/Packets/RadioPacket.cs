using Enocean.Core;
using Enocean.Core.Constants;
using Microsoft.Extensions.Logging;

namespace Enocean.Core.Packets
{
    public class RadioPacket : Packet
    {
        public RadioPacket(PACKET packet_type, List<byte> data, List<byte>? optional = null) : base(packet_type, data, optional)
        {

        }
        public List<byte> Destination { get; set; } = new List<byte> { 0xFF, 0xFF, 0xFF, 0xFF };
        public int Dbm { get; set; } = 0;
        public List<byte> Sender { get; set; } = new List<byte> { 0xFF, 0xFF, 0xFF, 0xFF };
        public bool Learn { get; set; } = true;
        public virtual bool ContainsEep { get; set; } = false;
        public uint SenderToUint => Utils.CombineHex(Sender);
        public string SenderToHex => Utils.ToHexString(Sender);
        public uint DestinationToUint => Utils.CombineHex(Destination);
        public string DestinationToHex => Utils.ToHexString(Destination);

        public override string ToString()
        {
            string packet_str = base.ToString();
            return $"{SenderToHex}->{DestinationToHex} ({Dbm} dBm): {packet_str}";
        }

        public static Packet CreateRadio(RORG rorg, int rorg_func, int rorg_type, int? direction = null, int? command = null,
            List<byte>? destination = null, List<byte>? sender = null, bool learn = false, Dictionary<string, object>? kwargs = null)
        {
            return Create(PACKET.RADIO_ERP1, rorg, rorg_func, rorg_type, direction, command, destination, sender, learn, kwargs);
        }

        public override Dictionary<string, StateItem> Parse()
        {
            Destination = m_optional.Skip(1).Take(4).ToList();
            if (m_optional?.Count > 5)
                Dbm = -m_optional[5];
            Sender = m_data.Skip(m_data.Count - 5).Take(4).ToList();
            Learn = true;

            m_rorg = (RORG)m_data[0];

            if (m_rorg == RORG.BS1)
            {
                Learn = !BitData[^DB0.BIT_3];
            }
            if (m_rorg == RORG.BS4)
            {
                Learn = !BitData[^DB0.BIT_3];
                if (Learn)
                {
                    ContainsEep = BitData[^DB0.BIT_7];
                    if (ContainsEep)
                    {
                        m_rorg_func = Utils.FromBitArray(BitData.Skip(BitData.Length - DB3.BIT_7).Take(6));
                        m_rorg_type = Utils.FromBitArray(BitData.Skip(BitData.Length - DB3.BIT_1).Take(7));
                        m_rorg_manufacturer = Utils.FromBitArray(BitData.Skip(BitData.Length - DB2.BIT_2).Take(11));
                        logger?.LogDebug($"learn received, EEP detected, RORG: 0x{(byte)m_rorg}, FUNC: 0x{(byte)m_rorg_func}, TYPE: 0x{(byte)m_rorg_type}, Manufacturer: 0x{(byte)m_rorg_manufacturer}");
                    }
                }
            }

            return base.Parse();
        }
    }
}
