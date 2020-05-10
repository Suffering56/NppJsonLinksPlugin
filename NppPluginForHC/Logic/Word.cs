using System;
using System.Collections.Generic;

namespace NppPluginForHC.Logic
{
    public class Word
    {
        private const string WordSeparatorShort = "<<";
        private static readonly string[] WordSeparator = {WordSeparatorShort};

        private readonly string _word;
        private readonly Word _parent;

        public static Word Parse(string fullWordStr)
        {
            string[] split = fullWordStr.Split(WordSeparator, StringSplitOptions.None);

            Word parent = null;
            for (int i = split.Length - 1; i >= 0; i--)
            {
                var wordStr = split[i];
                var word = new Word(wordStr, parent);
                parent = word;
            }

            return parent;
        }

        private Word(string word, Word parent)
        {
            _word = word;
            _parent = parent;
        }

        private bool Equals(Word other)
        {
            return _word == other._word && Equals(_parent, other._parent);
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
                return ((_word != null ? _word.GetHashCode() : 0) * 397) ^ (_parent != null ? _parent.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            Word cur = this;
            string result = cur._word;
            while (cur._parent != null)
            {
                cur = cur._parent;
                result += WordSeparatorShort + cur._word;
            }

            return result;
        }
    }
}