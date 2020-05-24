using System;
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

        private static Mode _mode = AppConstants.DefaultLoggerMode;
        private static string _infoPathPrefix = null;
        private static string _errorPathPrefix = null;

        public static void SetMode(Mode mode, string prefix)
        {
            _infoPathPrefix = null;
            _errorPathPrefix = null;

            if (mode == Mode.ENABLED && string.IsNullOrEmpty(prefix))
            {
                _mode = Mode.ONLY_ERRORS;
                ErrorMsgBox($"Settings.LogPathPrefix could not be empty when loggerMode is {Mode.ENABLED.ToString()}. Logger mode will set to: {Mode.ONLY_ERRORS}");
                return;
            }

            if (mode == Mode.ENABLED)
            {
                if (!Directory.Exists(prefix))
                {
                    ErrorMsgBox($"Cannot enable logger, because directory={prefix} not exist. Logger mode will set to {Mode.ONLY_ERRORS})");
                    _mode = Mode.ONLY_ERRORS;
                    return;
                }

                _infoPathPrefix = prefix + "out.log";
                _errorPathPrefix = prefix + "error.log";
            }

            _mode = mode;
        }

        internal static void ErrorMsgBox(string errorMsg)
        {
            if (_mode == Mode.DISABLED) return;
            MessageBox.Show("Plugin error: " + errorMsg, Main.PluginName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        [Conditional("TRACE_ALL")]
        internal static void Info(string msg)
        {
            if (_mode != Mode.ENABLED) return;

            Debug.WriteLine(msg);
            WriteLogInto(_infoPathPrefix, msg);
        }

        [Conditional("TRACE_ALL")]
        internal static void Error(string msg)
        {
            ErrorMsgBox(msg);
            if (_mode != Mode.ENABLED) return;

            Debug.WriteLine("ERROR:" + msg);
            WriteLogInto(_errorPathPrefix, msg);
        }

        internal static void Error(Exception ex)
        {
            ErrorMsgBox(ex.Message);
            if (_mode != Mode.ENABLED) return;

            Debug.WriteLine($"{ex.Message}\r\n{ex.StackTrace}");

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("\n");
            stringBuilder.Append($"{DateTime.Now.Year}-{DateTime.Now.Month:00}-{DateTime.Now.Day:00}");
            stringBuilder.Append($"{DateTime.Now.Hour:00}-{DateTime.Now.Minute:00}-{DateTime.Now.Second:00}");
            stringBuilder.Append(ex.Message);
            stringBuilder.Append(ex.StackTrace);
            var errorMsg = stringBuilder.ToString();

            WriteLogInto(_errorPathPrefix, errorMsg);
        }

        private static readonly string TracePId = Process.GetCurrentProcess().Id.ToString("X04");

        private static void WriteLogInto(string logFilePath, string msg)
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
                ErrorMsgBox($"Error while attempting to write into log file (loggerMode will set to {Mode.ONLY_ERRORS}), message:\n" + ex.Message);
                SetMode(Mode.ONLY_ERRORS, null);
            }
        }
    }
}