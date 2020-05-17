using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace NppPluginForHC.Core
{
    public static class Utils
    {
        public static void ExecuteDelayed(Action runnable, int delay)
        {
            new Thread(o =>
                {
                    Thread.Sleep(delay);
                    runnable.Invoke();
                }
            ).Start();
        }


        //TODO не учитываются строки с нецифрами и небуквами
        private const string TokenValuePattern = "^.*\"[PROPERTY_NAME]\"\\s*:\\s*\"?([\\w|\\.]+)\"?\\s*";

        public static string ExtractTokenValueByLine(string lineText, string propertyName)
        {
            string pattern = new StringBuilder(TokenValuePattern).Replace("[PROPERTY_NAME]", propertyName).ToString();

            var match = new Regex(pattern).Match(lineText);
            if (!match.Success) return null;

            var matchGroup = match.Groups[1];
            return matchGroup.Success
                ? matchGroup.Value
                : null;
        }
    }
}