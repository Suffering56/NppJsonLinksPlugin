using System;
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

        public IniConfig Clone()
        {
            IniConfig origin = this;

            return new IniConfig
            {
                SettingsJsonUri = origin.SettingsJsonUri,

                LogsDir = origin.LogsDir,
                LoggerMode = origin.LoggerMode,

                HighlightingEnabled = origin.HighlightingEnabled,
                SoundEnabled = origin.SoundEnabled,
                JumpToLineDelay = origin.JumpToLineDelay,
                MappingDefaultFilePath = origin.MappingDefaultFilePath
            };
        }

        public void Save()
        {
            IniConfigParser.Save(this);
        }
    }

    public static class IniConfigParser
    {
        private static string _iniFilePath;

        private const string SECTION_GLOBAL = "global";
        private const string SECTION_LOGGER = "logger";
        private const string SECTION_OVERRIDE = "override_settings";

        public static IniConfig Parse(string iniFilePath)
        {
            _iniFilePath = iniFilePath;

            var settingsJsonUri = ReadString(SECTION_GLOBAL, "settings_uri");
            if (settingsJsonUri == null) throw new Exception("cannot read property from config.ini: \"settings_uri\".");

            return new IniConfig
            {
                SettingsJsonUri = settingsJsonUri,

                LogsDir = ReadString(SECTION_LOGGER, "logs_dir"),
                LoggerMode = ReadLoggerMode(SECTION_LOGGER, "logger_mode", _iniFilePath, AppConstants.DEFAULT_LOGGER_MODE),

                HighlightingEnabled = ReadBool(SECTION_OVERRIDE, "highlighting_enabled"),
                SoundEnabled = ReadBool(SECTION_OVERRIDE, "sound_enabled"),
                JumpToLineDelay = ReadInt(SECTION_OVERRIDE, "jump_to_line_delay"),
                MappingDefaultFilePath = ReadString(SECTION_OVERRIDE, "mapping_default_file_path")
            };
        }

        internal static void Save(IniConfig config)
        {
            WriteString(SECTION_GLOBAL, "settings_uri", config.SettingsJsonUri);

            WriteString(SECTION_LOGGER, "logs_dir", config.LogsDir);
            WriteString(SECTION_LOGGER, "logger_mode", config.LoggerMode);

            WriteString(SECTION_OVERRIDE, "highlighting_enabled", config.HighlightingEnabled);
            WriteString(SECTION_OVERRIDE, "sound_enabled", config.SoundEnabled);
            WriteString(SECTION_OVERRIDE, "jump_to_line_delay", config.JumpToLineDelay);
            WriteString(SECTION_OVERRIDE, "mapping_default_file_path", config.MappingDefaultFilePath);
        }

        private static void WriteString(string section, string propertyName, object value)
        {
            if (value == null) return;
            Win32.WritePrivateProfileString(section, propertyName, value.ToString(), _iniFilePath);
        }


        private static string ReadString(string section, string propertyName)
        {
            StringBuilder sb = new StringBuilder(Win32.MAX_PATH);
            var result = Win32.GetPrivateProfileString(section, propertyName, null, sb, sb.Capacity, _iniFilePath);
            return result > 0
                ? sb.ToString()
                : null;
        }

        private static bool? ReadBool(string section, string propertyName)
        {
            var str = ReadString(section, propertyName);
            if (bool.TryParse(str, out bool result))
            {
                return result;
            }

            return null;
        }

        private static int? ReadInt(string section, string propertyName)
        {
            var str = ReadString(section, propertyName);
            if (int.TryParse(str, out int result))
            {
                return result;
            }

            return null;
        }

        private static Logger.Mode ReadLoggerMode(string section, string propertyName, string iniFilePath, Logger.Mode defaultValue)
        {
            var rawLoggerMode = ReadString(section, propertyName);

            if (rawLoggerMode == null || !Enum.TryParse(rawLoggerMode, true, out Logger.Mode loggerMode))
            {
                Logger.Error($"cannot read {section}.{propertyName} from config=\"{iniFilePath}\". Logger mode will set to: {defaultValue}", null, true);
                return defaultValue;
            }

            return loggerMode;
        }
    }
}