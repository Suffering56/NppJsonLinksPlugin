namespace NppPluginForHC.PluginInfrastructure
{
    public interface IExtendedScintillaGateway : IScintillaGateway
    {
        int PositionToLine(int position);

        int LineToPosition(int line);

        string GetTextFromPosition(int startPosition, int length);
        
        string GetTextFromPositionSafe(int currentPosition, int length);

        string GetCurrentWord();
        
        int GetCurrentLine();
    }
}