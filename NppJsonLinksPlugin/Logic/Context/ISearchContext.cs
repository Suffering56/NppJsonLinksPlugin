namespace NppJsonLinksPlugin.Logic.Context
{
    public interface ISearchContext
    {
        bool IsValid();
        
        string GetSelectedWord();

        Property GetSelectedProperty();

        bool MatchesWith(Word expectedWord);
    }
}