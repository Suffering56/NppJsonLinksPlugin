using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace NppJsonLinksPlugin.Core
{
    public static class Logger
    {
        public enum Mode
        {
            ENABLED,
            ONLY_ERRORS,
            DISABLED,
        }

        private static Mode _mode = AppConstants.DEFAULT_LOGGER_MODE;
        private static string _infoPathPrefix = null;
        private static string _errorPathPrefix = null;

        private const int FREQUENT_ERRORS_COUNT_LIMIT = 15;
        private const int FREQUENT_ERRORS_TIME_LIMIT_SECONDS = 5;
        private static readonly List<int> LastErrorTimesList = new List<int>();
        private static readonly object Lock = new object();

        public static void SetMode(Mode mode, string prefix)
        {
            _infoPathPrefix = null;
            _errorPathPrefix = null;

            if (mode == Mode.ENABLED && string.IsNullOrEmpty(prefix))
            {
                _mode = Mode.ONLY_ERRORS;
                ShowErrorDialog($"Settings.LogPathPrefix could not be empty when loggerMode is {Mode.ENABLED.ToString()}. Logger mode will set to: {Mode.ONLY_ERRORS}");
                return;
            }

            if (mode == Mode.ENABLED)
            {
                if (!Directory.Exists(prefix))
                {
                    ShowErrorDialog($"Cannot enable logger, because directory={prefix} not exist. Logger mode will set to {Mode.ONLY_ERRORS})");
                    _mode = Mode.ONLY_ERRORS;
                    return;
                }

                _infoPathPrefix = prefix + "out.log";
                _errorPathPrefix = prefix + "error.log";
            }

            _mode = mode;
        }

        internal static void Info(string msg)
        {
            if (_mode != Mode.ENABLED) return;

            Debug.WriteLine(msg);
            WriteToFile(_infoPathPrefix, msg);
        }

        public static void Warn(string msg)
        {
            Info($"WARN: {msg}");
        }

        public static void Fail(string msg)
        {
            Info($"FAIL: {msg}");
        }

        internal static void Error(string msg, Exception e = null, bool showMsgBox = false)
        {
            OnError();

            msg = $"Plugin \"{Main.PLUGIN_NAME}\" ERROR: {msg}";

            if (showMsgBox)
            {
                ShowErrorDialog(msg);
            }

            if (_mode != Mode.ENABLED) return;

            Debug.WriteLine(msg);
            WriteToFile(_errorPathPrefix, msg);
            PrintStackTrace(e);
        }

        private static void ShowErrorDialog(string errorMsg)
        {
            if (_mode == Mode.DISABLED) return;
            MsgBox(errorMsg, MessageBoxIcon.Error);
        }

        private static void MsgBox(string msg, MessageBoxIcon icon)
        {
            MessageBox.Show(msg, Main.PLUGIN_NAME, MessageBoxButtons.OK, icon);
        }

        private static void PrintStackTrace(Exception e)
        {
            if (e == null || _mode != Mode.ENABLED) return;

            Debug.WriteLine($"{e.StackTrace}");

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("\n");
            stringBuilder.Append($"{DateTime.Now.Year}-{DateTime.Now.Month:00}-{DateTime.Now.Day:00}");
            stringBuilder.Append($"{DateTime.Now.Hour:00}-{DateTime.Now.Minute:00}-{DateTime.Now.Second:00}");
            stringBuilder.Append(e.StackTrace);
            var errorMsg = stringBuilder.ToString();

            WriteToFile(_errorPathPrefix, errorMsg);
        }

        private static readonly string TracePId = Process.GetCurrentProcess().Id.ToString("X04");

        private static void WriteToFile(string logFilePath, string msg)
        {
            try
            {
                using TextWriter w = new StreamWriter(logFilePath, true);
                StackTrace stackTrace = new StackTrace();
                MethodBase methodBase = stackTrace.GetFrame(1).GetMethod();
                w.WriteLine($"{DateTime.Now.ToLongTimeString()}:{DateTime.Now.Millisecond:000} <{TracePId}>");
                w.Write($"{new String(' ', (stackTrace.FrameCount - 1) * 3)}[{methodBase.Name}] {msg}");
            }
            catch (Exception ex)
            {
                ShowErrorDialog($"Error while attempting to write into log file (loggerMode will set to {Mode.ONLY_ERRORS}), message:\n" + ex.Message);
                if (_mode != Mode.DISABLED)
                {
                    SetMode(Mode.ONLY_ERRORS, null);
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
    }
}