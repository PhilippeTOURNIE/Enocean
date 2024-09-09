using Enocean.Core;
using Enocean.Core.Constants;
using System.Text;

namespace Enocean.Console
{
    public record Material
    {
        public string Description;
        public RORG Rorg;
        public int RorgFunc;
        public int RorgType;
        public List<byte> Sender;
        public Dictionary<string, StateItem> State;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Rorg: {Rorg}");
            sb.AppendLine($"Func : {Utils.ToHexString((byte)RorgFunc)} Type : {Utils.ToHexString((byte)RorgType)} Description : {Description}");
            sb.AppendLine($"Sender : {Utils.ToHexString(Sender)}");
            foreach (var item in State)
            {
                sb.AppendLine($"item : {item.Key} state : {item.Value}");
            }
            return sb.ToString();
        }
    }
}
