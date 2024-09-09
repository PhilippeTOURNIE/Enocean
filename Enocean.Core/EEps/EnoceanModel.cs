using System.Xml.Serialization;
namespace Enocean.Core.EEps
{
    public interface IRaw
    {
        public int Offset { get; set; }
        public int Size { get; set; }
        public string? Shortcut { get; set; }
    }

    [XmlRoot(ElementName = "item")]
    public class Item
    {
        [XmlAttribute(AttributeName = "description")]
        public string? Description { get; set; }
        [XmlAttribute(AttributeName = "value")]
        public int Value { get; set; }
    }

    [XmlRoot(ElementName = "enum")]
    public class Enum : IRaw
    {
        [XmlElement(ElementName = "item")]
        public List<Item> Item { get; set; } = new List<Item>();
        [XmlAttribute(AttributeName = "description")]
        public string? Description { get; set; }
        [XmlAttribute(AttributeName = "shortcut")]
        public string? Shortcut { get; set; }
        [XmlAttribute(AttributeName = "offset")]
        public int Offset { get; set; }
        [XmlAttribute(AttributeName = "size")]
        public int Size { get; set; }
        [XmlElement(ElementName = "rangeitem")]
        public List<Rangeitem> Rangeitem { get; set; } = new List<Rangeitem>();
        [XmlAttribute(AttributeName = "unit")]
        public string? Unit { get; set; }
    }

    [XmlRoot(ElementName = "data")]
    public class Data
    {
        [XmlElement(ElementName = "enum")]
        public List<Enum> Enum { get; set; } = new List<Enum>();
        [XmlElement(ElementName = "status")]
        public List<Status> Status { get; set; } = new List<Status>();
        [XmlElement(ElementName = "value")]
        public List<Value> Value { get; set; } = new List<Value>();
        [XmlAttribute(AttributeName = "command")]
        public string? Command { get; set; }
        [XmlAttribute(AttributeName = "bits")]
        public int Bits { get; set; }
        [XmlAttribute(AttributeName = "direction")]
        public string? Direction { get; set; }
    }

    [XmlRoot(ElementName = "profile")]
    public class Profile
    {
        [XmlElement(ElementName = "data")]
        public List<Data> Data { get; set; } = new List<Data>();
        [XmlAttribute(AttributeName = "type")]
        public string? Type { get; set; }
        [XmlAttribute(AttributeName = "description")]
        public string? Description { get; set; }
        [XmlElement(ElementName = "command")]
        public Command? Command { get; set; }
    }

    [XmlRoot(ElementName = "profiles")]
    public class Profiles
    {
        [XmlElement(ElementName = "profile")]
        public List<Profile> Profile { get; set; } = new List<Profile>();
        [XmlAttribute(AttributeName = "func")]
        public string? Func { get; set; }
        [XmlAttribute(AttributeName = "description")]
        public string? Description { get; set; }
    }

    [XmlRoot(ElementName = "status")]
    public class Status : IRaw
    {
        [XmlAttribute(AttributeName = "description")]
        public string? Description { get; set; }
        [XmlAttribute(AttributeName = "shortcut")]
        public string? Shortcut { get; set; }
        [XmlAttribute(AttributeName = "offset")]
        public int Offset { get; set; }
        [XmlAttribute(AttributeName = "size")]
        public int Size { get; set; }
    }

    [XmlRoot(ElementName = "rangeitem")]
    public class Rangeitem
    {
        [XmlAttribute(AttributeName = "description")]
        public string? Description { get; set; }
        [XmlAttribute(AttributeName = "start")]
        public string? Start { get; set; }
        [XmlAttribute(AttributeName = "end")]
        public string? End { get; set; }
    }

    [XmlRoot(ElementName = "telegram")]
    public class Telegram
    {
        [XmlElement(ElementName = "profiles")]
        public List<Profiles> Profiles { get; set; } = new List<Profiles>();
        [XmlAttribute(AttributeName = "rorg")]
        public string? Rorg { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string? Type { get; set; }
        [XmlAttribute(AttributeName = "description")]
        public string? Description { get; set; }

    }

    [XmlRoot(ElementName = "range")]
    public class Range
    {
        [XmlElement(ElementName = "min")]
        public string? Min { get; set; }
        [XmlElement(ElementName = "max")]
        public string? Max { get; set; }
    }

    [XmlRoot(ElementName = "scale")]
    public class Scale
    {
        [XmlElement(ElementName = "min")]
        public string? Min { get; set; }
        [XmlElement(ElementName = "max")]
        public string? Max { get; set; }
    }

    [XmlRoot(ElementName = "value")]
    public class Value : IRaw
    {
        [XmlElement(ElementName = "range")]
        public Range? Range { get; set; }
        [XmlElement(ElementName = "scale")]
        public Scale? Scale { get; set; }
        [XmlAttribute(AttributeName = "description")]
        public string? Description { get; set; }
        [XmlAttribute(AttributeName = "shortcut")]
        public string? Shortcut { get; set; }
        [XmlAttribute(AttributeName = "offset")]
        public int Offset { get; set; }
        [XmlAttribute(AttributeName = "size")]
        public int Size { get; set; }
        [XmlAttribute(AttributeName = "unit")]
        public string? Unit { get; set; }
    }

    [XmlRoot(ElementName = "command")]
    public class Command : IRaw
    {
        [XmlElement(ElementName = "item")]
        public List<Item> Item { get; set; } = new List<Item>();
        [XmlAttribute(AttributeName = "description")]
        public string? Description { get; set; }
        [XmlAttribute(AttributeName = "shortcut")]
        public string? Shortcut { get; set; }
        [XmlAttribute(AttributeName = "offset")]
        public int Offset { get; set; }
        [XmlAttribute(AttributeName = "size")]
        public int Size { get; set; }
    }

    [XmlRoot(ElementName = "telegrams")]
    public class Telegrams
    {
        [XmlElement(ElementName = "telegram")]
        public List<Telegram> Telegram { get; set; } = new List<Telegram>();
        [XmlAttribute(AttributeName = "version")]
        public string? Version { get; set; }
        [XmlAttribute(AttributeName = "major_version")]
        public string? Major_version { get; set; }
        [XmlAttribute(AttributeName = "minor_version")]
        public string? Minor_version { get; set; }
        [XmlAttribute(AttributeName = "revision")]
        public string? Revision { get; set; }
    }

}
