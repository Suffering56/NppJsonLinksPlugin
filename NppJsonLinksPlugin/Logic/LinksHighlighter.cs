using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NppJsonLinksPlugin.Configuration;
using NppJsonLinksPlugin.Core;
using NppJsonLinksPlugin.Logic.Context;
using NppJsonLinksPlugin.PluginInfrastructure.Gateway;

namespace NppJsonLinksPlugin.Logic
{
    public class LinksHighlighter
    {
        private const int HIGHLIGHT_INDICATOR_ID = 32;

        private const int STYLE_NONE = 5;
        private const int STYLE_UNDERLINE = 0;
        private const int STYLE_HOVER = 0;

        private readonly IScintillaGateway _gateway;
        private readonly Settings _settings;
        private List<Word> _expectedWords;

        private int _startVisibleLine = 0;
        private int _endVisibleLine = 0;
        private int _startVisiblePosition = 0;
        private int _endVisiblePosition = 0;
        private string _currentPath = null;

        private readonly Func<string, int, int, ISearchContext> _searchContextProvider;

        public LinksHighlighter(IScintillaGateway gateway, Settings settings)
        {
            _gateway = gateway;
            _settings = settings;
            _searchContextProvider = (word, initialLineIndex, indexOfSelectedWord) => new JsonSearchContext(word, gateway, initialLineIndex, indexOfSelectedWord);

            gateway.SetIndicatorStyle(HIGHLIGHT_INDICATOR_ID, STYLE_UNDERLINE);

            Update();
            HighlightVisibleText();
        }

        private bool Update()
        {
            var startVisibleLine = _gateway.GetFirstVisibleLine();
            var endVisibleLine = startVisibleLine + _gateway.LinesOnScreen() - 1;

            var currentPath = StringSupport.NormalizePath(_gateway.GetFullCurrentPath());
            var startVisiblePosition = _gateway.LineToPosition(startVisibleLine);
            var endVisiblePosition = _gateway.GetLineEndPosition(endVisibleLine).Value;

            bool changed = currentPath != _currentPath;
            if (changed)
            {
                _expectedWords = _settings.Mapping
                    .Select(item => item.Src)
                    .Where(src => src.MatchesWithPath(currentPath))
                    .Select(src => src.Word)
                    .ToList();
            }

            changed = changed
                      || startVisibleLine != _startVisibleLine
                      || endVisibleLine != _endVisibleLine
                      || startVisiblePosition != _startVisiblePosition
                      || endVisiblePosition != _endVisiblePosition;

            _currentPath = currentPath;
            _startVisibleLine = startVisibleLine;
            _endVisibleLine = endVisibleLine;
            _startVisiblePosition = startVisiblePosition;
            _endVisiblePosition = endVisiblePosition;

            return changed;
        }


        public void UpdateUi()
        {
            if (Update())
            {
                HighlightVisibleText();
            }
        }

        private void HighlightVisibleText()
        {
            try
            {
                CleanCurrentFileHighlighting();
                if (_expectedWords.Count == 0) return;

                for (int lineIndex = _startVisibleLine; lineIndex <= _endVisibleLine; lineIndex++)
                {
                    var isEscape = false;
                    StringBuilder currentWord = null;

                    var lineText = _gateway.GetLineText(lineIndex);

                    for (int i = 0; i < lineText.Length; i++)
                    {
                        var ch = lineText[i];

                        if (!isEscape && ch == '"')
                        {
                            if (currentWord == null)
                            {
                                currentWord = new StringBuilder();
                            }
                            else
                            {
                                string word = currentWord.ToString();
                                var wordLength = word.Length;
                                if (IsNeedToHighlightWord(word, lineIndex, i - wordLength))
                                {
                                    int position = _gateway.LineToPosition(lineIndex) + i - wordLength;
                                    HighlightWord(position, wordLength);
                                }

                                currentWord = null;
                            }
                        }
                        else if (currentWord != null)
                        {
                            if (ch.IsPartOfWord())
                                currentWord.Append(ch);
                            else
                                currentWord = null;
                        }

                        isEscape = !isEscape && ch == '\\';
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        private bool IsNeedToHighlightWord(string word, int lineIndex, int indexOfWord)
        {
            var searchContext = _searchContextProvider.Invoke(word, lineIndex, indexOfWord);
            var contextProperty = searchContext.GetSelectedProperty();

            if (contextProperty == null) return false;

            // нас интересует только подсветка property name, value - игнорируем
            if (contextProperty.Name != word) return false;

            foreach (var srcWord in _expectedWords)
            {
                if (searchContext.MatchesWith(srcWord))
                {
                    return true;
                }
            }

            return false;
        }

        private void HighlightWord(int indexOfWord, int wordLength)
        {
            _gateway.ApplyIndicatorStyleForRange(HIGHLIGHT_INDICATOR_ID, indexOfWord, wordLength);
        }

        private void CleanCurrentFileHighlighting()
        {
            _gateway.ClearIndicatorStyleForRange(HIGHLIGHT_INDICATOR_ID, 0, _gateway.GetTextLength());
        }
    }
}