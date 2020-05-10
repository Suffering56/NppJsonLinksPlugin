namespace NppPluginForHC.Logic
{
    public struct JumpLocation
    {
        public string FilePath;
        public int Line;

        public JumpLocation(string filePath, int line)
        {
            FilePath = filePath;
            Line = line;
        }
    }
}