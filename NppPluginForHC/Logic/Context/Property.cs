namespace NppPluginForHC.Logic.Context
{
    public class Property
    {
        public readonly string Name;    
        public readonly string Value;

        public Property(string propertyName, string propertyValue)
        {
            Name = propertyName;
            Value = propertyValue;
        }
    }
}