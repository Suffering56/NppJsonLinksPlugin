namespace NppPluginForHC.PluginInfrastructure.Gateway
{
    public partial interface IScintillaGateway
    {
        void OpenFile(string filePath);

        string GetFullCurrentPath();

        int PositionToLine(int position);

        int LineToPosition(int line);

        string GetTextFromPosition(int startPosition, int length, int linesAdded);

        string GetTextFromPositionSafe(int currentPosition, int length, int linesAdded);

        string GetCurrentWord();

        int GetCurrentLine();

        void JumpToLine(int line);
    }
}