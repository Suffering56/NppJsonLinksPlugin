using NppPluginForHC.Core;
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

        public string GetTokenValue(string propertyName)
        {
            string currentLineText = Gateway.GetLineText(Gateway.GetCurrentLine());
            return Utils.ExtractTokenValueByLine(currentLineText, propertyName);
        }
    }
}