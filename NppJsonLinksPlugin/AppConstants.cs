using NppJsonLinksPlugin.Core;

namespace NppJsonLinksPlugin
{
    public static class AppConstants
    {
        public const string INI_CONFIG_NAME = "config.ini";
        public const string WORD_SEPARATOR = "<<<";
        public const string ROOT_PROPERTY_NAME = "ROOT";

        public const string SETTINGS_JSON_URI_PLACEHOLDER = "https://raw.githubusercontent.com/Suffering56/NppJsonLinksPlugin/master/NppJsonLinksPlugin/settings.json";
        
        public const string DEFAULT_LOGGER_PATH = "C:/";
        public const Logger.Mode DEFAULT_LOGGER_MODE = Logger.Mode.ONLY_ERRORS;
        
        public const string MAPPING_PATH_PLACEHOLDER = "D:/projects/shelter/gd_data";
        
        public const int DEFAULT_PROCESSING_HIGHLIGHTING_LINES_LIMIT = 10000;
        public const int DEFAULT_JUMP_TO_LINE_DELAY = 100;
    }
}