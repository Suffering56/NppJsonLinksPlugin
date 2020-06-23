using System.IO;
using NppJsonLinksPlugin.Core;

namespace NppJsonLinksPlugin
{
    public static class AppConstants
    {
        public const string INI_CONFIG_NAME = "config.ini";
        public const string WORD_SEPARATOR = "<<<";
        public const string ROOT_PROPERTY_NAME = "ROOT";

        // public static readonly string DEFAULT_SETTINGS_URI = Path.GetFullPath(GetPluginPath() + "/settings.json");
        public static readonly string DEFAULT_SETTINGS_URI = "https://raw.githubusercontent.com/Suffering56/NppJsonLinksPlugin/master/NppJsonLinksPlugin/settings.json";
        public static readonly string DEFAULT_LOGGER_PATH = Path.GetFullPath(GetPluginPath() + "/logs");

        public const Logger.Mode DEFAULT_LOGGER_MODE = Logger.Mode.ENABLED;

        public const string MAPPING_PATH_PLACEHOLDER = "D:/projects/shelter/gd_data";
        public const int DEFAULT_JUMP_TO_LINE_DELAY = 100;

        private static string GetPluginPath()
        {
            return "plugins/" + Main.PLUGIN_NAME;
        }
    }
}