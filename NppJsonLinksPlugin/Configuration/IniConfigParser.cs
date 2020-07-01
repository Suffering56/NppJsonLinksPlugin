using System;
using System.Text;
using NppJsonLinksPlugin.Core;
using NppJsonLinksPlugin.PluginInfrastructure;
using static NppJsonLinksPlugin.AppConstants;

namespace NppJsonLinksPlugin.Configuration
{
    public class IniConfig
    {
        public string MappingRemoteUrl;
        public Logger.Mode LoggerMode;
        public string WorkingDirectory;
        public int MappingDefaultSrcOrder;
        public bool HighlightingEnabled;
        public int HighlightingTimerInterval;
        public int JumpToLineDelay;
        public bool SoundEnabled;

        public IniConfig Clone()
        {
            IniConfig origin = this;

            return new IniConfig
            {
                MappingRemoteUrl = origin.MappingRemoteUrl,
                LoggerMode = origin.LoggerMode,
                WorkingDirectory = origin.WorkingDirectory,
                MappingDefaultSrcOrder = origin.MappingDefaultSrcOrder,
                HighlightingEnabled = origin.HighlightingEnabled,
                HighlightingTimerInterval = origin.HighlightingTimerInterval,
                JumpToLineDelay = origin.JumpToLineDelay,
                SoundEnabled = origin.SoundEnabled
            };
        }

        public bool Save()
        {
            return IniConfigParser.Save(this);
        }
    }

    public static class IniConfigParser
    {
        private const string SECTION_COMMON = "common";

        private const string MAPPING_REMOTE_URL_1 = "remote_mapping_url";
        private const string LOGGER_MODE_2 = "logger_mode";
        private const string WORKING_DIRECTORY_3 = "working_directory";
        private const string MAPPING_DEFAULT_SRC_ORDER_4 = "mapping_default_src_order";
        private const string HIGHLIGHTING_ENABLED_5 = "highlighting_enabled";
        private const string HIGHLIGHTING_TIMER_INTERVAL_6 = "highlighting_timer_interval";
        private const string JUMP_TO_LINE_DELAY_7 = "jump_to_line_delay";
        private const string SOUND_ENABLED_8 = "sound_enabled";

        public static IniConfig Parse()
        {
            return new IniConfig
            {
                MappingRemoteUrl = ReadStringRequired(MAPPING_REMOTE_URL_1),
                LoggerMode = ReadLoggerMode(LOGGER_MODE_2, Defaults.LOGGER_MODE),
                WorkingDirectory = ReadStringRequired(WORKING_DIRECTORY_3),
                MappingDefaultSrcOrder = ReadInt(MAPPING_DEFAULT_SRC_ORDER_4, Defaults.MAPPING_DEFAULT_SRC_ORDER),
                HighlightingEnabled = ReadBool(HIGHLIGHTING_ENABLED_5, Defaults.HIGHLIGHTING_ENABLED),
                HighlightingTimerInterval = ReadInt(HIGHLIGHTING_TIMER_INTERVAL_6, Defaults.HIGHLIGHTING_TIMER_INTERVAL),
                JumpToLineDelay = ReadInt(JUMP_TO_LINE_DELAY_7, Defaults.JUMP_TO_LINE_DELAY),
                SoundEnabled = ReadBool(SOUND_ENABLED_8, Defaults.SOUND_ENABLED)
            };
        }

        internal static bool Save(IniConfig config)
        {
            try
            {
                Logger.Info("Saving config.ini...");

                WriteString(MAPPING_REMOTE_URL_1, config.MappingRemoteUrl);
                WriteString(LOGGER_MODE_2, config.LoggerMode);
                WriteString(WORKING_DIRECTORY_3, config.WorkingDirectory);
                WriteString(MAPPING_DEFAULT_SRC_ORDER_4, config.MappingDefaultSrcOrder);
                WriteString(HIGHLIGHTING_ENABLED_5, config.HighlightingEnabled);
                WriteString(HIGHLIGHTING_TIMER_INTERVAL_6, config.HighlightingTimerInterval);
                WriteString(JUMP_TO_LINE_DELAY_7, config.JumpToLineDelay);
                WriteString(SOUND_ENABLED_8, config.SoundEnabled);
                return true;
            }
            catch (Exception e)
            {
                Logger.Error("couldn't save config.ini", e, true);
                return false;
            }
        }

        private static void WriteString(string propertyName, object value)
        {
            if (value == null) return;
            Win32.WritePrivateProfileString(SECTION_COMMON, propertyName, value.ToString(), INI_FILE_PATH);
        }

        private static string ReadStringRequired(string propertyName)
        {
            return ReadString(propertyName, () => $"cannot read {propertyName} from config=\"{INI_FILE_PATH}\". {Main.PLUGIN_NAME} will be disabled.");
        }

        private static string ReadString(string propertyName, Func<string> errorMsgSupplier = null)
        {
            StringBuilder sb = new StringBuilder(Win32.MAX_PATH);
            var result = Win32.GetPrivateProfileString(SECTION_COMMON, propertyName, null, sb, sb.Capacity, INI_FILE_PATH);

            if (result > 0) return sb.ToString();
            if (errorMsgSupplier == null) return null;
            throw new Exception(errorMsgSupplier.Invoke());
        }

        private static bool ReadBool(string propertyName, bool defaultValue)
        {
            var str = ReadString(propertyName);
            return ConvertUtils.ToBool(str, defaultValue);
        }

        private static int ReadInt(string propertyName, int defaultValue)
        {
            var str = ReadString(propertyName);
            return ConvertUtils.ToInt(str, defaultValue);
        }

        private static Logger.Mode ReadLoggerMode(string propertyName, Logger.Mode defaultValue)
        {
            return ConvertUtils.ToLoggerMode(
                ReadString(propertyName),
                defaultValue,
                () => $"cannot read {propertyName} from config=\"{INI_FILE_PATH}\". Logger mode will set to: {defaultValue}"
            );
        }
    }
}