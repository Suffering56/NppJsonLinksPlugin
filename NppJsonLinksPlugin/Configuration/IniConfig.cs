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

        public bool? SoundEnabled = null;
        public int? JumpToLineDelay = null;
        public string MappingDefaultFilePath = null;

        private const string SectionGlobal = "global";
        private const string SectionOverride = "override_settings";

        public void Load()
        {
            string iniFilePath = Path.GetFullPath($"plugins/{Main.PLUGIN_NAME}/{AppConstants.INI_CONFIG_NAME}");


            SettingsJsonUri = ReadString(SectionGlobal, "settings_uri", iniFilePath, true);
            LogsDir = ReadString(SectionGlobal, "logs_dir", iniFilePath, false);
            LoggerMode = ReadLoggerMode("logger_mode", iniFilePath);

            //TODO: можно сделать красиво, но мне лень
            var soundEnabledStr = ReadString(SectionOverride, "sound_enabled", iniFilePath, false);
            if (bool.TryParse(soundEnabledStr, out bool boolResult))
            {
                SoundEnabled = boolResult;
            }

            var jumpToLineDelayStr = ReadString(SectionOverride, "jump_to_line_delay", iniFilePath, false);
            if (int.TryParse(jumpToLineDelayStr, out int intResult))
            {
                JumpToLineDelay = intResult;
            }

            MappingDefaultFilePath = ReadString(SectionOverride, "mapping_default_file_path", iniFilePath, false);
        }

        private static string ReadString(string section, string propertyName, string iniFilePath, bool required)
        {
            StringBuilder sb = new StringBuilder(Win32.MAX_PATH);
            var result = Win32.GetPrivateProfileString(section, propertyName, null, sb, sb.Capacity, iniFilePath);
            if (result <= 0)
            {
                if (required)
                {
                    Logger.Error($"cannot read property from config.ini: {propertyName}");
                    Main.DisablePlugin();
                }

                return null;
            }

            return sb.ToString();
        }

        private static Logger.Mode ReadLoggerMode(string propertyName, string iniFilePath)
        {
            var rawLoggerMode = ReadString(SectionGlobal, propertyName, iniFilePath, false);
            if (rawLoggerMode == null) return AppConstants.DEFAULT_LOGGER_MODE;

            Enum.TryParse(rawLoggerMode, true, out Logger.Mode loggerMode);
            return loggerMode;
        }
    }
}