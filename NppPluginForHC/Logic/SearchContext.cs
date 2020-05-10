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

        public object GetTokenValue()
        {
            //TODO: STUB
            return "MOBRANGER";    
        }
    }
}