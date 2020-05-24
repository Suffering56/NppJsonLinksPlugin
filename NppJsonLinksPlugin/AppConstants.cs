using NppJsonLinksPlugin.Core;

namespace NppJsonLinksPlugin
{
    public static class AppConstants
    {
        private const bool DebugEnabled = true;
        public const string RootPropertyName = "ROOT";
        public const int DefaultJumpToLineDelay = 100;
        public const Logger.Mode DefaultLoggerMode = Logger.Mode.ONLY_ERRORS;
        
        public static bool IsDebugEnabled()
        {
            return DebugEnabled;
        }
    }
}