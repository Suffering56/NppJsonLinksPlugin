using System.Collections.Generic;

namespace NppPluginForHC.Logic.Parser
{
    public delegate void OnExpectedValueFound(Word word, int line, string value);

    public interface IDocumentParser
    {
        void ParseValidDocument(string filePath, ICollection<Word> expectedWords, OnExpectedValueFound onValueFound);

        void ParseInvalidDocument(string filePath, ICollection<Word> expectedWords, OnExpectedValueFound onValueFound);
    }
}