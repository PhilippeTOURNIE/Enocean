namespace Enocean.Core
{
    public class StateItem
    {
        public string? description { get; set; }
        public string? unit { get; set; }
        public string? value { get; set; }
        public string? raw_value { get; set; }

        public override string ToString()
        {
            return $"description :{description} {value} {unit} (rawvalue :{raw_value})";
        }
    }
}
