using NppJsonLinksPlugin.Core;

namespace NppJsonLinksPlugin
{
    public static class AppConstants
    {
        // public const string DEFAULT_MAPPING_PATH_PLACEHOLDER = "DEFAULT_MAPPING_PATH_PLACEHOLDER";
        // public const string LOGGER_PATH_PLACEHOLDER = "LOGGER_PATH_PLACEHOLDER";
        // public const string SETTINGS_JSON_URI_PLACEHOLDER = "SETTINGS_JSON_URI_PLACEHOLDER";
        
        public const string INI_CONFIG_NAME = "config.ini";
        public const string WORD_SEPARATOR = "<<<";
        public const string ROOT_PROPERTY_NAME = "ROOT";
        public const int DEFAULT_JUMP_TO_LINE_DELAY = 100;
        public const int DEFAULT_PROCESSING_HIGHLIGHTING_LINES_LIMIT = 10000;
        public const Logger.Mode DEFAULT_LOGGER_MODE = Logger.Mode.ONLY_ERRORS;
        
        public const string DEFAULT_MAPPING_PATH_PLACEHOLDER = "DEFAULT_MAPPING_PATH_PLACEHOLDER";
        public const string LOGGER_PATH_PLACEHOLDER = "LOGGER_PATH_PLACEHOLDER";
        public const string SETTINGS_JSON_URI_PLACEHOLDER = "SETTINGS_JSON_URI_PLACEHOLDER";
    }
}