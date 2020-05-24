using System;

namespace NppJsonLinksPlugin.Logic
{
    public class Word
    {
        private const string WordSeparator = "<<";
        private static readonly string[] WordSeparatorForSplit = {WordSeparator};

        public readonly string WordString;
        public readonly Word Parent;

        public static Word Parse(string fullWordStr)
        {
            string[] split = fullWordStr.Split(WordSeparatorForSplit, StringSplitOptions.None);

            Word parent = null;
            for (int i = split.Length - 1; i >= 0; i--)
            {
                var wordStr = split[i];
                var word = new Word(wordStr, parent);
                parent = word;
            }

            return parent;
        }

        private Word(string wordString, Word parent)
        {
            WordString = wordString;
            Parent = parent;
        }

        public bool IsComplex()
        {
            return Parent != null;
        }

        private bool Equals(Word other)
        {
            return WordString == other.WordString && Equals(Parent, other.Parent);
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
                return ((WordString != null ? WordString.GetHashCode() : 0) * 397) ^ (Parent != null ? Parent.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            Word cur = this;
            string result = cur.WordString;
            while (cur.Parent != null)
            {
                cur = cur.Parent;
                result += WordSeparator + cur.WordString;
            }

            return result;
        }
    }
}