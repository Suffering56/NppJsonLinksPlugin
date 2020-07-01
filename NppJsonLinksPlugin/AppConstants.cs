using System.IO;
using NppJsonLinksPlugin.Core;

namespace NppJsonLinksPlugin
{
    public static class AppConstants
    {
        public const string WORD_SEPARATOR = "<<<";
        public const string ROOT_PROPERTY_NAME = "ROOT";

        public static readonly string INI_FILE_PATH = GetPluginPath("config.ini");
        public static readonly string MAPPING_LOCAL_DIR = GetPluginPath("");
        public static readonly string MAPPING_LOCAL_PATH = GetPluginPath("mapping.json");
        public static readonly string LOGS_PATH = GetPluginPath("logs/");

        private static string GetPluginPath(string postfix)
        {
            return Path.GetFullPath($"plugins/{Main.PLUGIN_NAME}/{postfix}");
        }

        public static class Defaults
        {
            //1
            public const string MAPPING_REMOTE_URL = "https://raw.githubusercontent.com/Suffering56/NppJsonLinksPlugin/master/NppJsonLinksPlugin/mapping.json";

            //2
            public const Logger.Mode LOGGER_MODE = Logger.Mode.ENABLED_WITHOUT_ALERTS;

            //3
            public const string WORKING_DIRECTORY = "D:/projects/shelter/gd_data";

            //4
            public const int MAPPING_DEFAULT_SRC_ORDER = 999999999;

            //5
            public const bool HIGHLIGHTING_ENABLED = true;

            //6
            public const int HIGHLIGHTING_TIMER_INTERVAL = 500;

            //7
            public const int JUMP_TO_LINE_DELAY = 0;

            //8
            public const bool SOUND_ENABLED = false;
        }

        public static class Placeholders
        {
            public const string MAPPING_REMOTE_URL = Defaults.MAPPING_REMOTE_URL;
            public const string WORKING_DIRECTORY = Defaults.WORKING_DIRECTORY;
            public const string MAPPING_DEFAULT_SRC_ORDER = "999999999";
            public const string HIGHLIGHTING_TIMER_INTERVAL = "500";
            public const string JUMP_TO_LINE_DELAY = "100";
        }
    }
}