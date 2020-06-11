using NppJsonLinksPlugin.Core;

namespace NppJsonLinksPlugin
{
    public static class AppConstants
    {
        public const string IniConfigName = "config.ini";
        public const string WordSeparator = "<<<";
        public const string RootPropertyName = "ROOT";
        public const int DefaultJumpToLineDelay = 100;
        public const Logger.Mode DefaultLoggerMode = Logger.Mode.ONLY_ERRORS;

        private const bool DebugEnabled = true;

        public static bool IsDebugEnabled()
        {
            return DebugEnabled;
        }
    }
}