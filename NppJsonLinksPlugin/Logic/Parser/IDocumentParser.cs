using System.Collections.Generic;

namespace NppJsonLinksPlugin.Logic.Parser
{
    public delegate void ValueConsumer(Word word, int line, string value);

    public interface IDocumentParser
    {
        void ParseValidDocument(string filePath, ICollection<Word> expectedWords, ValueConsumer valueConsumer);

        void ParseInvalidDocument(string filePath, ICollection<Word> expectedWords, ValueConsumer valueConsumer);
    }
}