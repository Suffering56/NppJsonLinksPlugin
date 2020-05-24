using System;
using System.Threading;

namespace NppJsonLinksPlugin.Core
{
    public static class ThreadSupport
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
    
    public static class StringSupport
    {
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

    public static class CharSupport
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