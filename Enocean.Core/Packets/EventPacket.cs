using Enocean.Core;
using Enocean.Core.Constants;

namespace Enocean.Core.Packets
{
    public class EventPacket : Packet
    {
        public EventPacket(PACKET packet_type, List<byte> data, List<byte>? optional)
            : base(packet_type, data, optional)
        {

        }
        private int m_event;
        public int Event => m_event;
        private List<byte> m_event_data = new List<byte>();
        public List<byte> EventData => m_event_data;

        public override Dictionary<string, StateItem> Parse()
        {
            m_event = m_data[0];
            m_event_data = m_data.Skip(1).ToList();
            return base.Parse();
        }
    }
}
