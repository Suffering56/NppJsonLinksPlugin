using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace NppPluginForHC
{
    public static class Log
    {
        private const string TraceFilePath = "C:/NppPluginForHC/output.txt";
        private const string ErrorFilePath = "C:/NppPluginForHC/error.txt";

        private static readonly string TracePId = Process.GetCurrentProcess().Id.ToString("X04");

        [Conditional("TRACE_ALL")]
        internal static void Out(string msg)
        {
            Debug.WriteLine(msg);
            WriteLogInto(TraceFilePath, msg);
        }
        [Conditional("TRACE_ALL")]
        internal static void Error(string msg)
        {
            Debug.WriteLine(msg);
            WriteLogInto(ErrorFilePath, msg);
        }

        private static void WriteLogInto(string logFilePath, string msg)
        {
            try
            {
                using (TextWriter w = new StreamWriter(logFilePath, true))
                {
                    StackTrace stackTrace = new StackTrace();
                    MethodBase methodBase = stackTrace.GetFrame(1).GetMethod();
                    w.WriteLine(
                        $"{DateTime.Now.ToLongTimeString()}:{DateTime.Now.Millisecond:000} <{TracePId}> {new String(' ', (stackTrace.FrameCount - 1) * 3)}[{methodBase.Name}] {msg}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while attempting to write into log file:\n" + ex.Message,
                    "NppPluginForHC", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        internal static void Error(Exception ex)
        {
            try
            {
                using (TextWriter w = new StreamWriter(ErrorFilePath, true))
                {
                    w.WriteLine(
                        $"\n{DateTime.Now.Year}-{DateTime.Now.Month:00}-{DateTime.Now.Day:00} {DateTime.Now.Hour:00}-{DateTime.Now.Minute:00}-{DateTime.Now.Second:00}:\n" +
                        "====================");
                    w.WriteLine(ex.Message);
                    w.WriteLine(ex.StackTrace);
                }

                MessageBox.Show("Owing to unfortunate circumstances an error with the following message occured:\n\n"
                                + "\"" + ex.Message + "\"\n\n"
                                + "Hence a logfile has been written to the NppPluginForHC folder.\n"
                                + "Please post its content in the forum, if you think it's worth being fixed.\n"
                                + "Sorry for the inconvenience.",
                    "NppPluginForHC", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error while attempting to write error logfile:\n" + e.Message,
                    "NppPluginForHC", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}