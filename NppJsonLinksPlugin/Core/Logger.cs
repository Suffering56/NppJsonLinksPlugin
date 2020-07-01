using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace NppJsonLinksPlugin.Core
{
    public static class Logger
    {
        public enum Mode
        {
            ENABLED = 0,
            ENABLED_WITHOUT_ALERTS = 1,
            DISABLED_WITH_ALERTS = 2,
            DISABLED = 3,
        }

        private static Mode _mode = AppConstants.Defaults.LOGGER_MODE;
        private const int FREQUENT_ERRORS_COUNT_LIMIT = 15;
        private const int FREQUENT_ERRORS_TIME_LIMIT_SECONDS = 5;
        private static readonly List<int> LastErrorTimesList = new List<int>();
        private static readonly object Lock = new object();

        public static void SetMode(Mode mode)
        {
            _mode = mode;

            if (IsFileRecordingEnabled() && !Directory.Exists(AppConstants.LOGS_PATH))
            {
                try
                {
                    Directory.CreateDirectory(AppConstants.LOGS_PATH);
                }
                catch (Exception)
                {
                    ErrorBox($"Cannot enable logger. Can't create not existing logs directory={AppConstants.LOGS_PATH}. Logger mode will set to {Mode.DISABLED_WITH_ALERTS})");
                    _mode = Mode.DISABLED_WITH_ALERTS;
                }
            }
        }

        internal static void Info(string msg)
        {
            WriteToConsole(msg);
            WriteToFile(InfoLogFilePath(), msg);
        }

        public static void Warn(string msg)
        {
            Info($"WARN: {msg}");
        }

        public static void Fail(string msg)
        {
            Info($"FAIL: {msg}");
        }

        internal static void Error(string msg, Exception e = null, bool showAlert = false)
        {
            OnError();
            if (_mode == Mode.DISABLED) return;

            msg = $"Plugin \"{Main.PLUGIN_NAME}\" ERROR: {msg}";

            if (showAlert)
            {
                ErrorBox(msg);
            }

            WriteToConsole(msg);
            WriteToFile(ErrorLogFilePath(), msg);
            PrintStackTrace(e);
        }

        private static void PrintStackTrace(Exception e)
        {
            if (e == null) return;

            WriteToConsole($"{e.StackTrace}");

            if (!IsFileRecordingEnabled()) return;

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("\n");
            stringBuilder.Append(DateUtils.CurrentDateTimeStr());
            stringBuilder.Append(e.StackTrace);
            var errorMsg = stringBuilder.ToString();

            WriteToFile(ErrorLogFilePath(), errorMsg);
        }

        private static readonly string TracePId = Process.GetCurrentProcess().Id.ToString("X04");

        private static void WriteToFile(string logFilePath, string msg)
        {
            if (!IsFileRecordingEnabled()) return;

            try
            {
                using TextWriter w = new StreamWriter(logFilePath, true);
                w.WriteLine($"{DateUtils.CurrentDateTimeStr()}:<{TracePId}>: {msg}");
            }
            catch (Exception ex)
            {
                ErrorBox($"Error while attempting to write into log file (loggerMode will set to {Mode.DISABLED_WITH_ALERTS}), message:\n" + ex.Message);
                if (IsAlertsEnabled())
                {
                    SetMode(Mode.DISABLED_WITH_ALERTS);
                }
            }
        }

        /**
         * Если за последние FREQUENT_ERRORS_TIME_LIMIT_SECONDS случится больше FREQUENT_ERRORS_COUNT_LIMIT ошибок, то плагин нужно отключить
         */
        private static void OnError()
        {
            lock (Lock)
            {
                // нельзя завязываться на if (mode == DISABLED) return; потому что пользователь может выключить логгер намерено и ошибки в плагине начнут тормозить NPP
                if (Main.IsPluginDisabled) return;

                var currentUts = DateUtils.CurrentUts();

                for (var i = LastErrorTimesList.Count - 1; i >= 0; i--)
                {
                    var prevErrorTime = LastErrorTimesList[i];
                    if (currentUts - prevErrorTime > FREQUENT_ERRORS_TIME_LIMIT_SECONDS)
                    {
                        LastErrorTimesList.RemoveAt(i);
                    }
                }

                if (LastErrorTimesList.Count > FREQUENT_ERRORS_COUNT_LIMIT)
                {
                    MsgBox("Too many errors occured last time, plugin will be disabled", MessageBoxIcon.Error);
                    Main.DisablePlugin();
                    return;
                }

                LastErrorTimesList.Add(currentUts);
            }
        }

        [Conditional("DEBUG")]
        private static void WriteToConsole(string msg)
        {
            Debug.WriteLine(msg);
        }

        private static void MsgBox(string msg, MessageBoxIcon icon)
        {
            MessageBox.Show(msg, Main.PLUGIN_NAME, MessageBoxButtons.OK, icon);
        }

        public static void InfoBox(string msg)
        {
            MsgBox(msg, MessageBoxIcon.Information);
        }

        private static void ErrorBox(string errorMsg)
        {
            if (!IsAlertsEnabled()) return;
            MsgBox(errorMsg, MessageBoxIcon.Error);
        }

        private static string InfoLogFilePath()
        {
            return Path.Combine(AppConstants.LOGS_PATH, $"{DateUtils.CurrentDateStr()}_out.log");
        }

        private static string ErrorLogFilePath()
        {
            return Path.Combine(AppConstants.LOGS_PATH, $"{DateUtils.CurrentDateStr()}_err.log");
        }

        private static bool IsFileRecordingEnabled()
        {
            return _mode == Mode.ENABLED || _mode == Mode.ENABLED_WITHOUT_ALERTS;
        }

        private static bool IsAlertsEnabled()
        {
            return _mode == Mode.ENABLED || _mode == Mode.DISABLED_WITH_ALERTS;
        }
    }
}