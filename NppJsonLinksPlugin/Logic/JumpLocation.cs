namespace NppJsonLinksPlugin.Logic
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

        public override string ToString()
        {
            return $"file=<{FilePath}>, line={Line}";
        }
    }
}