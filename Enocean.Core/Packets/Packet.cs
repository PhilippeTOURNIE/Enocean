using Enocean.Core;
using Enocean.Core.Constants;
using Enocean.Core.EEps;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Text;

namespace Enocean.Core.Packets
{
    /// <summary>
    ///  Base class for Packet.
    /// Mainly used for for packet generation and
    /// Packet.parse_msg(buf) for parsing message.
    /// parse_msg() returns subclass, if one is defined for the data type.
    /// </summary>
    public class Packet
    {
        protected static ILogger? logger;
        protected PACKET m_packet_type;
        protected RORG m_rorg;
        protected int m_rorg_func;
        protected int m_rorg_type;
        protected int m_rorg_manufacturer;
        protected DateTime m_received;
        protected List<byte> m_data;
        protected List<byte> m_optional;
        protected byte m_status = 0;
        protected Dictionary<string, StateItem> m_parsed = new Dictionary<string, StateItem>();
        protected int m_repeater_count = 0;
        protected List<Data> m_profile;
        protected EEP m_EEP = new EEP(logger);

        public EEP EEPObject => m_EEP;
        public RORG Rorg { get => m_rorg; set => m_rorg = value; }
        public int RorgFunc => m_rorg_func;
        public int RorgType => m_rorg_type;
        public byte Status => m_status;
        public int RepeaterCount => m_repeater_count;
        public List<byte> Optional { get => m_optional; set => m_optional = value; }
        public List<byte> Data { get => m_data; set => m_data = value; }
        public Dictionary<string, StateItem> Parsed => m_parsed;
        public PACKET PacketType => m_packet_type;
        public int RorgManufacturer => m_rorg_manufacturer;
        public DateTime Received { get => m_received; set => m_received = value; }
        public Packet(PACKET packet_type, List<byte> data, List<byte>? optional = null)
        {
            m_packet_type = packet_type;
            m_rorg = (int)RORG.UNDEFINED;

            if (data is null)
            {
                logger?.LogError("Replacing Packet.data with default value.");
                m_data = new List<byte>();
            }
            else
                m_data = data;

            if (optional is null)
            {
                logger?.LogError("Replacing Packet.optional with default value.");
                m_optional = new List<byte>();
            }
            else
                m_optional = optional;

            m_status = 0;
            m_parsed = new Dictionary<string, StateItem>();
            m_repeater_count = 0;
            m_profile = new List<Data>();

            Parse();
        }

        public override string ToString()
        {
            var str = new StringBuilder($"Packet type : {Utils.ToHexString((byte)m_packet_type)}, data: {Utils.ToHexString(m_data)}, option : {Utils.ToHexString(m_optional)}");
            str.AppendLine();
            foreach (var item in m_parsed)
            {
                str.AppendLine($"item : {item.Key} state : {item.Value}");
            }
            return str.ToString();
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object? obj)
        {
            if (obj is Packet other)
            {
                return m_packet_type == other.m_packet_type &&
                       m_rorg == other.m_rorg &&
                       m_data.SequenceEqual(other.m_data) &&
                       m_optional.SequenceEqual(other.m_optional);
            }
            return false;
        }

        public BitArray BitData
        {
            get
            {
                return Utils.ToBitArray(m_data.Skip(1).Take(m_data.Count - 6).ToArray());
            }
            set
            {
                var newData = value.FromBiteArray();
                for (int byteIndex = 0; byteIndex < m_data.Count - 6; byteIndex++)
                {
                    m_data[byteIndex + 1] = newData[byteIndex];
                }
            }
        }
        public BitArray BitStatus
        {
            get
            {
                return Utils.ToBitArray(new byte[] { m_status });
            }
            set
            {
                m_status = Utils.FromBitArray(value);
            }
        }

        public static (PARSE_RESULT, List<byte>, Packet?) ParseMsg(List<byte> buf)
        {
            if (!buf.Contains(0x55))
            {
                return (PARSE_RESULT.INCOMPLETE, new List<byte>(), null);
            }

            logger?.LogInformation(Utils.ToHexString(buf));

            buf = buf.Skip(buf.IndexOf(0x55)).ToList();

            int data_len = 0;
            int opt_len = 0;
            try
            {
                data_len = buf[1] << 8 | buf[2];
                opt_len = buf[3];
            }
            catch
            {
                return (PARSE_RESULT.INCOMPLETE, buf, null);
            }

            int msg_len = 6 + data_len + opt_len + 1;
            if (buf.Count < msg_len)
            {
                return (PARSE_RESULT.INCOMPLETE, buf, null);
            }

            List<byte> msg = buf.Take(msg_len).ToList();
            buf = buf.Skip(msg_len).ToList();

            var packet_type = (PACKET)msg[4];
            List<byte> data = msg.Skip(6).Take(data_len).ToList();
            List<byte> opt_data = msg.Skip(6 + data_len).Take(opt_len).ToList();

            if (msg[5] != CRC8.Calc(msg.Skip(1).Take(4).ToArray()))
            {
                logger?.LogError("Header CRC error!");
                return (PARSE_RESULT.CRC_MISMATCH, buf, null);
            }
            if (msg[6 + data_len + opt_len] != CRC8.Calc(msg.Skip(6).Take(data_len + opt_len).ToArray()))
            {
                logger?.LogError("Data CRC error!");
                return (PARSE_RESULT.CRC_MISMATCH, buf, null);
            }

            Packet packet;
            if (packet_type == PACKET.RADIO_ERP1)
            {
                if ((RORG)data[0] == RORG.UTE)
                {
                    packet = new UTETeachInPacket(packet_type, data, opt_data);
                }
                else
                {
                    packet = new RadioPacket(packet_type, data, opt_data);
                }
            }
            else if (packet_type == PACKET.RESPONSE)
            {
                packet = new ResponsePacket(packet_type, data, opt_data);
            }
            else if (packet_type == PACKET.EVENT)
            {
                packet = new EventPacket(packet_type, data, opt_data);
            }
            else
            {
                packet = new Packet(packet_type, data, opt_data);
            }

            return (PARSE_RESULT.OK, buf, packet);
        }

        public static Packet Create(PACKET packet_type, RORG rorg, int rorg_func, int rorg_type, int? direction = null, int? command = null,
            List<byte>? destination = null, List<byte>? sender = null, bool learn = false, Dictionary<string, object>? kwargs = null)
        {
            if (packet_type != PACKET.RADIO_ERP1)
            {
                throw new ArgumentException("Packet type not supported by this function.");
            }

            if (!new[] { RORG.RPS, RORG.BS1, RORG.BS4, RORG.VLD }.Contains(rorg))
            {
                throw new ArgumentException("RORG not supported by this function.");
            }

            if (destination == null)
            {
                logger?.LogWarning("Replacing destination with broadcast address.");
                destination = new List<byte> { 0xFF, 0xFF, 0xFF, 0xFF };
            }

            if (sender == null)
            {
                logger?.LogWarning("Replacing sender with default address.");
                sender = new List<byte> { 0xDE, 0xAD, 0xBE, 0xEF };
            }

            if (destination == null || destination.Count != 4)
            {
                throw new ArgumentException("Destination must be a list containing 4 (numeric) values.");
            }

            if (sender == null || sender.Count != 4)
            {
                throw new ArgumentException("Sender must be a list containing 4 (numeric) values.");
            }

            Packet? packet = new Packet(packet_type, new List<byte>(), new List<byte>());
            packet.m_rorg = rorg;
            packet.m_data = new List<byte> { (byte)(int)packet.m_rorg };
            packet.SelectEEP(rorg_func, rorg_type, direction, command);

            if (new[] { RORG.RPS, RORG.BS1 }.Contains(rorg))
            {
                packet.m_data.Add(0);
            }
            else if (rorg == RORG.BS4)
            {
                packet.m_data.AddRange(new List<byte>() { 0, 0, 0, 0 });
            }
            else
            {
                packet.m_data.AddRange(Enumerable.Repeat<byte>(0, packet.m_profile.First().Bits));
            }
            packet.m_data.AddRange(sender);
            packet.m_data.Add(0);
            packet.m_optional = new List<byte> { 3 };
            packet.m_optional.AddRange(destination);
            packet.m_optional.AddRange(new List<byte> { 0xFF, 0 });

            if (command.HasValue && kwargs != null)
            {
                kwargs["CMD"] = (byte)command.Value;
            }

            packet.SetEEP(kwargs);
            if (new[] { RORG.BS1, RORG.BS4 }.Contains(rorg) && !learn)
            {
                if (rorg == RORG.BS1)
                {
                    packet.m_data[1] |= 1 << 3;
                }
                if (rorg == RORG.BS4)
                {
                    packet.m_data[4] |= 1 << 3;
                }
            }
            packet.m_data[packet.m_data.Count - 1] = packet.m_status;

            (PARSE_RESULT res, List<byte> bb, packet) = ParseMsg(packet.Build());
            packet!.m_rorg = rorg;
            packet!.ParseEEP(rorg_func, rorg_type, direction, command);
            return packet;
        }

        public virtual Dictionary<string, StateItem> Parse()
        {
            if (new[] { RORG.RPS, RORG.BS1, RORG.BS4, RORG.UTE }.Contains(m_rorg))
            {
                m_status = m_data[m_data.Count - 1];
            }
            if (m_rorg == RORG.VLD)
            {
                m_status = m_optional[m_optional.Count - 1];
            }

            if (new[] { RORG.RPS, RORG.BS1, RORG.BS4 }.Contains(m_rorg))
            {
                m_repeater_count = Utils.FromBitArray(BitStatus) & 0x0F;
            }
            return m_parsed;
        }

        public bool SelectEEP(int rorg_func, int rorg_type, int? direction = null, int? command = null)
        {
            m_rorg_func = rorg_func;
            m_rorg_type = rorg_type;
            var profile = EEPObject.FindProfil(BitData, m_rorg, rorg_func, rorg_type, direction, command);
            if (profile != null)
                m_profile = profile;

            return profile != null;
        }

        public Dictionary<string, StateItem> ParseEEP(int? rorg_func = null, int? rorg_type = null, int? direction = null, int? command = null)
        {
            if (rorg_func.HasValue && rorg_type.HasValue)
            {
                SelectEEP(rorg_func.Value, rorg_type.Value, direction, command);
            }

            m_parsed = EEPObject.GetValues(m_profile, BitData, BitStatus);
            return m_parsed;
        }

        public void SetEEP(Dictionary<string, object>? data)
        {
            (var _bit_data, var _bit_status) = EEPObject.SetValues(m_profile, BitData, BitStatus, data);
            BitData = _bit_data;
            BitStatus = _bit_status;
        }

        public List<byte> Build()
        {
            byte data_length = (byte)m_data.Count;
            List<byte> ords = new List<byte> { 0x55, (byte)(data_length >> 8 & 0xFF), (byte)(data_length & 0xFF), (byte)m_optional.Count, (byte)m_packet_type };
            ords.Add(CRC8.Calc(ords.Skip(1).Take(4).ToArray()));
            ords.AddRange(m_data);
            ords.AddRange(m_optional);
            ords.Add(CRC8.Calc(ords.Skip(6).ToArray()));
            return ords;
        }
        internal static void SetLogger(ILogger? logger)
        {
            Packet.logger = logger;
        }
    }
}

