using System;
using System.IO;
using System.Text;
using NppJsonLinksPlugin.Core;
using NppJsonLinksPlugin.PluginInfrastructure;

namespace NppJsonLinksPlugin.Configuration
{
    public class IniConfig
    {
        public string SettingsJsonUri = null;
        public string LogsDir = null;
        public Logger.Mode LoggerMode = AppConstants.DEFAULT_LOGGER_MODE;

        public bool? HighlightingEnabled = null;
        public bool? SoundEnabled = null;
        public int? JumpToLineDelay = null;
        public string MappingDefaultFilePath = null;

        private const string SECTION_GLOBAL = "global";
        private const string SECTION_LOGGER = "logger";
        private const string SECTION_OVERRIDE = "override_settings";

        public bool Reload()
        {
            string iniFilePath = Path.GetFullPath($"plugins/{Main.PLUGIN_NAME}/{AppConstants.INI_CONFIG_NAME}");

            SettingsJsonUri = ReadString(SECTION_GLOBAL, "settings_uri", iniFilePath, true);
            if (SettingsJsonUri == null)
            {
                Logger.Error($"cannot read property from config.ini: \"settings_uri\". Plugin will be disabled");
                return false;
            }

            LogsDir = ReadString(SECTION_LOGGER, "logs_dir", iniFilePath, false);
            LoggerMode = ReadLoggerMode(SECTION_LOGGER, "logger_mode", iniFilePath, AppConstants.DEFAULT_LOGGER_MODE);

            HighlightingEnabled = ReadBool(SECTION_OVERRIDE, "highlighting_enabled", iniFilePath, false);
            SoundEnabled = ReadBool(SECTION_OVERRIDE, "sound_enabled", iniFilePath, false);
            JumpToLineDelay = ReadInt(SECTION_OVERRIDE, "jump_to_line_delay", iniFilePath, false);
            MappingDefaultFilePath = ReadString(SECTION_OVERRIDE, "mapping_default_file_path", iniFilePath, false);
            return true;
        }

        private static string ReadString(string section, string propertyName, string iniFilePath, bool required)
        {
            StringBuilder sb = new StringBuilder(Win32.MAX_PATH);
            var result = Win32.GetPrivateProfileString(section, propertyName, null, sb, sb.Capacity, iniFilePath);
            return result > 0
                ? sb.ToString()
                : null;
        }

        private static bool? ReadBool(string section, string propertyName, string iniFilePath, bool required)
        {
            var str = ReadString(section, propertyName, iniFilePath, required);
            if (bool.TryParse(str, out bool result))
            {
                return result;
            }

            return null;
        }

        private static int? ReadInt(string section, string propertyName, string iniFilePath, bool required)
        {
            var str = ReadString(section, propertyName, iniFilePath, required);
            if (int.TryParse(str, out int result))
            {
                return result;
            }

            return null;
        }

        private static Logger.Mode ReadLoggerMode(string section, string propertyName, string iniFilePath, Logger.Mode defaultValue)
        {
            var rawLoggerMode = ReadString(section, propertyName, iniFilePath, false);

            if (rawLoggerMode == null || !Enum.TryParse(rawLoggerMode, true, out Logger.Mode loggerMode))
            {
                Logger.Error($"cannot read {section}.{propertyName} from config=\"{iniFilePath}\". Logger mode will set to: {defaultValue}");
                return defaultValue;
            }

            return loggerMode;
        }
    }
}