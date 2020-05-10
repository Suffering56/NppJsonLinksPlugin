using NppPluginForHC.PluginInfrastructure;

namespace NppPluginForHC.Logic
{
    public struct SearchContext
    {
        public IExtendedScintillaGateway Gateway { get; }

        public SearchContext(IExtendedScintillaGateway gateway)
        {
            Gateway = gateway;
        }
    }
}