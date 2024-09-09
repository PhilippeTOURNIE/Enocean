using Enocean.Core;
using Enocean.Core.Constants;

namespace Enocean.Core.Packets
{
    public class ResponsePacket : Packet
    {
        public ResponsePacket(PACKET packet_type, List<byte> data, List<byte>? optional = null) : base(packet_type, data, optional)
        {

        }
        public int Response { get; set; }
        public List<byte> ResponseData { get; set; } = new List<byte>();

        public override Dictionary<string, StateItem> Parse()
        {
            Response = m_data[0];
            ResponseData = m_data.Skip(1).ToList();
            return base.Parse();
        }
    }
}
