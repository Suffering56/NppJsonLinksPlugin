using System.Diagnostics;
using NppPluginForHC.Core;
using NppPluginForHC.PluginInfrastructure;
using IScintillaGateway = NppPluginForHC.PluginInfrastructure.Gateway.IScintillaGateway;

namespace NppPluginForHC.Logic
{
    public delegate SearchContext SearchContextProvider();

    public class SearchContext
    {
        public IScintillaGateway Gateway { get; }

        public SearchContext(IScintillaGateway gateway)
        {
            Gateway = gateway;
        }

        public string GetTokenValue(string propertyName)
        {
            string currentLineText = Gateway.GetLineText(Gateway.GetCurrentLine());
            return JsonStringUtils.ExtractTokenValueByLine(currentLineText, propertyName);
        }

        public bool IsSelectedWordEqualsWith(Word expectedWord)
        {
            var selectedWord = Gateway.GetCurrentWord();
            Debug.Assert(expectedWord.WordString == selectedWord, $"initial expectedWord={expectedWord} string is not equal with selected word={selectedWord}");

            var line = Gateway.GetCurrentLine();
            var propertyName = expectedWord.WordString;

            Word parent = expectedWord;
            while ((parent = parent.Parent) != null)
            {
                var tokenResult = JsonStringUtils.GetParentToken(propertyName, line, Gateway.GetLineText);
                if (tokenResult.PropertyName != parent.WordString)
                {
                    return false;
                }

                propertyName = tokenResult.PropertyName;
                line = tokenResult.PropertyLine;
            }

            return true;
        }
    }
}