namespace NppPluginForHC.PluginInfrastructure
{
    public partial interface IScintillaGateway
    {
        int PositionToLine(int position);

        int LineToPosition(int line);

        string GetTextFromPosition(int startPosition, int length, int linesAdded);

        string GetTextFromPositionSafe(int currentPosition, int length, int linesAdded);

        string GetCurrentWord();

        int GetCurrentLine();
    }
}