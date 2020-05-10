namespace NppPluginForHC.Logic
{
    public class JumpLocation
    {
        public string FilePath { get; }
        public int Line { get; }

        public JumpLocation(string filePath, int line)
        {
            FilePath = filePath;
            Line = line;
        }
    }
}