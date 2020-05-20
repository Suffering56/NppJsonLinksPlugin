namespace NppPluginForHC.Logic.Context
{
    public interface ISearchContext
    {
        string GetTokenValue(string propertyName);

        bool MatchesWith(Word expectedWord);
    }
}