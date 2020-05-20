using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using NppPluginForHC.Logic.Parser.Json;
using NppPluginForHC.PluginInfrastructure.Gateway;

namespace NppPluginForHC.Logic.Context
{
    public class JsonSearchContext : ISearchContext
    {
        private readonly IScintillaGateway _gateway;

        //TODO не учитываются строки с нецифрами и небуквами
        private const string TokenValuePattern = "^.*\"[PROPERTY_NAME]\"\\s*:\\s*\"?([\\w|\\.]+)\"?\\s*";

        public JsonSearchContext(IScintillaGateway gateway)
        {
            _gateway = gateway;
        }

        public string GetTokenValue(string propertyName)
        {
            string currentLineText = _gateway.GetLineText(_gateway.GetCurrentLine());
            string pattern = new StringBuilder(TokenValuePattern).Replace("[PROPERTY_NAME]", propertyName).ToString();

            var match = new Regex(pattern).Match(currentLineText);
            if (!match.Success) return null;

            var matchGroup = match.Groups[1];
            return matchGroup.Success
                ? matchGroup.Value
                : null;
        }

        public bool MatchesWith(Word expectedWord)
        {
            var selectedWord = _gateway.GetCurrentWord();
            Debug.Assert(expectedWord.WordString == selectedWord, $"initial expectedWord={expectedWord} string is not equal with selected word={selectedWord}");

            var line = _gateway.GetCurrentLine();
            var propertyName = expectedWord.WordString;

            Word parent = expectedWord;
            while ((parent = parent.Parent) != null)
            {
                var tokenResult = JsonStringUtils.GetParentToken(propertyName, line, _gateway.GetLineText);
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