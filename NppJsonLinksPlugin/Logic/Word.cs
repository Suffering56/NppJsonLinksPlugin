using System;
using System.Text.RegularExpressions;
using NppJsonLinksPlugin.Core;

namespace NppJsonLinksPlugin.Logic
{
    public class Word
    {
        private const string WORD_SEPARATOR = AppConstants.WORD_SEPARATOR;
        private static readonly string[] WordSeparatorForSplit = {WORD_SEPARATOR};
        private const string SEVERAL_LETTERS_PATTERN = "[\\w_]*";
        private const string SINGLE_LETTERS_PATTERN = "[\\w_]?";

        public readonly Word Parent;
        private readonly string _wordString;
        private readonly string _wordStringPattern = null;

        public static Word ParseSrc(string fullWordStr, bool regexpEnabled)
        {
            string[] split = fullWordStr.Split(WordSeparatorForSplit, StringSplitOptions.None);

            Word parent = null;
            for (int i = split.Length - 1; i >= 0; i--)
            {
                var wordStr = split[i];
                var word = new Word(wordStr, parent, regexpEnabled);
                parent = word;
            }

            return parent;
        }

        public static Word ParseDst(string fullWordStr)
        {
            return ParseSrc(fullWordStr, false);
        }

        public string GetWordString()
        {
            return _wordString;
        }

        private Word(string wordString, Word parent, bool regexpEnabled)
        {
            _wordString = wordString;
            Parent = parent;

            if (StringSupport.IsValidWord(wordString))
            {
                _wordStringPattern = null;
            }
            else if (regexpEnabled)
            {
                _wordStringPattern = wordString;
            }
            else if (wordString.Contains("*") || wordString.Contains("?"))
            {
                _wordStringPattern = ToRegexp(wordString);
            }
        }

        private static string ToRegexp(string wordString)
        {
            wordString = wordString
                .Replace("*", SEVERAL_LETTERS_PATTERN)
                .Replace("?", SINGLE_LETTERS_PATTERN);
            return "^" + wordString + "$";
        }

        public bool IsComplex()
        {
            return Parent != null;
        }

        private bool Equals(Word other)
        {
            return _wordString == other._wordString && Equals(Parent, other.Parent);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((Word) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_wordString != null ? _wordString.GetHashCode() : 0) * 397) ^ (Parent != null ? Parent.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            Word cur = this;
            string result = "[" + cur._wordString;
            while (cur.Parent != null)
            {
                cur = cur.Parent;
                result += WORD_SEPARATOR + cur._wordString;
            }

            return result + "]";
        }

        public bool MatchesWith(string propertyName)
        {
            if (_wordStringPattern != null)
            {
                return Regex.IsMatch(propertyName, _wordStringPattern, RegexOptions.IgnoreCase);
            }

            return _wordString == propertyName;
        }
    }
}