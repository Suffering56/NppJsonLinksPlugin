namespace NppJsonLinksPlugin.Logic.Context
{
    public class Property
    {
        public readonly string Name;    
        public readonly string Value;
        
        public readonly int NameLineIndex;    // номер строки (line) на которой нашли propertyName
        public readonly int NameLineOffset;   // позиция в строке (line) с которой начинается propertyName


        public Property(string propertyName, int nameLineIndex, int nameLineOffset, string propertyValue)
        {
            Name = propertyName;
            NameLineIndex = nameLineIndex;
            NameLineOffset = nameLineOffset;
            Value = propertyValue;
        }
        
        public Property(PropertyNameLocation propertyNameLocation, string propertyValue)
        {
            Name = propertyNameLocation.PropertyName;
            NameLineIndex = propertyNameLocation.StopLineIndex;
            NameLineOffset = propertyNameLocation.StopLineOffset;
            Value = propertyValue;
        }

        public override string ToString()
        {
            return $"[{nameof(Name)}: {Name}, {nameof(Value)}: {Value}]";
        }
    }
}