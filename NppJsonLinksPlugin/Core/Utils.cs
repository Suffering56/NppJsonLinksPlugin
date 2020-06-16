using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace NppJsonLinksPlugin.Core
{
    public static class DateUtils
    {
        public static int CurrentUts()
        {
            return (int) (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }

    public static class ThreadUtils
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
    }

    public static class StringUtils
    {
        private const string VALID_WORD_PATTERN = "^[\\w_]*?$";

        public static bool IsValidWord(string str)
        {
            return Regex.IsMatch(str, VALID_WORD_PATTERN);
        }

        public static string NormalizePath(string path)
        {
            try
            {
                if (path == null) return null;
                if (path.Trim() == "*") return path;

                if (path.Contains("*"))
                {
                    path = path.Replace("*", "__ASTERISK__");
                    path = Path.GetFullPath(path);
                    path = path.Replace("__ASTERISK__", "*");
                    return path;
                }

                return Path.GetFullPath(path);
            }
            catch (Exception e)
            {
                Logger.Error("error path:", e, true);
                return null;
            }
        }

        public static string WithoutLast(this string str)
        {
            if (str == null) return null;
            if (str.Length == 0) return "";
            return str.Substring(0, str.Length - 1);
        }

        public static bool IsInteger(this string value)
        {
            return int.TryParse(value, out var ignore);
        }

        public static string Reverse(this string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }

    public static class CharUtils
    {
        public static bool IsWhiteSpace(this char checkedChar)
        {
            return char.IsWhiteSpace(checkedChar);
        }

        public static bool IsDigit(this char checkedChar)
        {
            return char.IsDigit(checkedChar);
        }

        public static bool IsDigitOrMinus(this char checkedChar)
        {
            return char.IsDigit(checkedChar) || checkedChar == '-';
        }

        public static bool IsLetter(this char checkedChar)
        {
            return char.IsLetter(checkedChar);
        }

        public static bool IsPartOfWord(this char checkedChar)
        {
            return char.IsLetter(checkedChar) || char.IsDigit(checkedChar) || checkedChar == '_';
        }

        public static bool IsOneOf(this char checkedChar, char expectedChar1, char expectedChar2)
        {
            return checkedChar == expectedChar1 || checkedChar == expectedChar2;
        }

        public static bool IsOneOf(this char checkedChar, char expectedChar1, char expectedChar2, char expectedChar3)
        {
            return checkedChar == expectedChar1 || checkedChar == expectedChar2 || checkedChar == expectedChar3;
        }

        public static bool IsWhiteSpaceOr(this char checkedChar, char expectedChar1)
        {
            return char.IsWhiteSpace(checkedChar) || checkedChar == expectedChar1;
        }

        public static bool IsWhiteSpaceOr(this char checkedChar, char expectedChar1, char expectedChar2)
        {
            return char.IsWhiteSpace(checkedChar) || checkedChar == expectedChar1 || checkedChar == expectedChar2;
        }

        public static bool IsWhiteSpaceOr(this char checkedChar, char expectedChar1, char expectedChar2, char expectedChar3)
        {
            return char.IsWhiteSpace(checkedChar) || checkedChar == expectedChar1 || checkedChar == expectedChar2 || checkedChar == expectedChar3;
        }
    }
}