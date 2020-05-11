using System.Text;
using System.Text.RegularExpressions;
using NppPluginForHC.PluginInfrastructure;

namespace NppPluginForHC.Logic
{
    public delegate SearchContext SearchContextProvider();

    public struct SearchContext
    {
        public IExtendedScintillaGateway Gateway { get; }

        public SearchContext(IExtendedScintillaGateway gateway)
        {
            Gateway = gateway;
        }

        // private static readonly StringBuilder TokenValuePattern = new StringBuilder("^.*\"[PROPERTY_NAME]\"\\s*:\\s*\"?(\\w+)\"?.*");
        private static readonly StringBuilder TokenValuePattern = new StringBuilder("^.*\"[PROPERTY_NAME]\"\\s*:\\s*\"?([\\w|\\.]+)\"?\\s*");    //TODO не учитываются строки с нецифрами и небуквами

        public string GetTokenValue(string propertyName)
        {
            string currentLineText = Gateway.GetLineText(Gateway.GetCurrentLine());
            string pattern = TokenValuePattern.Replace("[PROPERTY_NAME]", propertyName).ToString();
            
            Regex regex = new Regex(pattern);

            var match = regex.Match(currentLineText);
            if (!match.Success) return null;

            var matchGroup = match.Groups[1];
            return matchGroup.Success
                ? matchGroup.Value
                : null;
        }
    }
}