namespace NppJsonLinksPlugin.Logic.Context
{
    public class PropertyNameLocation
    {
        internal static readonly PropertyNameLocation Root = new PropertyNameLocation(AppConstants.RootPropertyName, 0, 0);

        internal readonly string PropertyName;
        internal readonly int StopLineIndex;
        internal readonly int StopLineOffset;

        public PropertyNameLocation(string propertyName, int stopLineIndex, int stopLineOffset)
        {
            PropertyName = propertyName;
            StopLineIndex = stopLineIndex;
            StopLineOffset = stopLineOffset;
        }

        public bool IsRoot()
        {
            return this == Root;
        }
    }
}