using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NppJsonLinksPlugin.Configuration;
using NppJsonLinksPlugin.Core;
using NppJsonLinksPlugin.Logic.Context;
using NppJsonLinksPlugin.Logic.Parser;
using NppJsonLinksPlugin.Logic.Parser.Json;
using static NppJsonLinksPlugin.Configuration.Settings;

namespace NppJsonLinksPlugin.Logic
{
    public class SearchEngine
    {
        private readonly IExtendedDictionary<MappingItem, DstFileContainer> _mappingToDstFileContainerMap;

        private string _currentFilePath = null;
        private bool _cacheEnabled;
        private readonly IDocumentParser _parser;

        public SearchEngine()
        {
            _mappingToDstFileContainerMap = new ExtendedDictionary<MappingItem, DstFileContainer>();
            _parser = new DefaultJsonParser();
        }

        public void Init(Settings settings, string currentFilePath)
        {
            _cacheEnabled = settings.CacheEnabled;
            var dstFilePathToDstWordsCache = new ExtendedDictionary<string, ISet<Word>>(); // dstFilePath -> ISet<dstWord>

            foreach (var mappingItem in settings.Mapping)
            {
                var dst = mappingItem.Dst;
                var dstFilePath = dst.FullPath;

                var fileDstWords = dstFilePathToDstWordsCache.ComputeIfAbsent(dstFilePath, key => new HashSet<Word>());
                fileDstWords.Add(dst.Word);
            }

            foreach (var mappingItem in settings.Mapping)
            {
                var dstFilePath = mappingItem.Dst.FullPath;
                var supportedWords = dstFilePathToDstWordsCache[dstFilePath];

                Debug.Assert(!_mappingToDstFileContainerMap.ContainsKey(mappingItem), $"outer container already contains mappingItem={mappingItem}");
                _mappingToDstFileContainerMap[mappingItem] = new DstFileContainer(_parser, dstFilePath, supportedWords);
            }

            SwitchContext(currentFilePath);
        }

        public void SwitchContext(string currentFilePath)
        {
            currentFilePath = StringSupport.NormalizePath(currentFilePath);
            if (currentFilePath == _currentFilePath) return; // контекст не изменился

            Logger.Info($"OnSwitchContext[changed={currentFilePath != _currentFilePath}]: <{_currentFilePath}> to <{currentFilePath}>");
            _currentFilePath = currentFilePath;
        }

        public void FireInsertText(int currentLine, int linesAdded, string insertedText)
        {
            // GetContainerByDstFilePath(_currentFilePath)?.OnContentChanged();
        }

        public void FireDeleteText(int currentLine, int linesDeleted)
        {
            //from = currentLine + 1, to =  currentLine - linesAdded
            // GetContainerByDstFilePath(_currentFilePath)?.OnContentChanged();
        }

        public void FireSaveFile()
        {
            GetContainerByDstFilePath(_currentFilePath)?.OnContentChanged();
        }

        public JumpLocation? FindDefinitionLocation(ISearchContext searchContext)
        {
            Logger.Info($"try find definition location for: selectedWord: {searchContext.GetSelectedWord()}");
            var property = searchContext.GetSelectedProperty();

            if (property == null)
            {
                Logger.Info($"FAIL: selected token not found for selected word: \"{searchContext.GetSelectedWord()}\"");
                return null;
            }

            var mappingItem = GetMappingItem(searchContext);
            if (mappingItem == null)
            {
                Logger.Info($"FAIL: mapping not found! selectedProperty={property} by selectedWord=\"{searchContext.GetSelectedWord()}\" does not match with any srcWord");
                return null;
            }

            var dstFileContainer = _mappingToDstFileContainerMap[mappingItem];
            dstFileContainer.InitIfNeeded(_cacheEnabled);

            var tokenValue = property.Value;
            var jumpLocation = dstFileContainer.FindDestinationLocation(mappingItem.Dst.Word, tokenValue);

            if (jumpLocation == null)
            {
                Logger.Info($"FAIL: could not find suitable location for mappingItem=[{mappingItem}] and tokenValue=<{tokenValue}>");
                return null;
            }

            Logger.Info($"SUCCESS: destination location successfully found for mappingItem=[{mappingItem}] and tokenValue={tokenValue}. JumpLocation=[{jumpLocation}]");
            return jumpLocation;
        }

        private MappingItem? GetMappingItem(ISearchContext searchContext)
        {
            foreach (var mappingItem in _mappingToDstFileContainerMap.Keys)
            {
                if (!mappingItem.Src.MatchesWithPath(_currentFilePath)) continue;

                if (searchContext.MatchesWith(mappingItem.Src.Word)) return mappingItem;
            }

            return null;
        }

        private DstFileContainer GetContainerByDstFilePath(string dstFilePath)
        {
            return _mappingToDstFileContainerMap.Values.FirstOrDefault(dstFileContainer => dstFileContainer.DstFilePath == dstFilePath);
        }

        private class DstFileContainer
        {
            private readonly IDocumentParser _parser;
            internal string DstFilePath { get; }
            private readonly IDictionary<Word, ValuesLocationContainer> _dstWordToValuesLocationContainer; // dstWord -> ValuesLocationContainer
            private readonly bool _hasComplexWords;
            private bool _inited;
            private bool _changed;

            internal DstFileContainer(IDocumentParser parser, string dstFilePath, ISet<Word> dstWords)
            {
                _parser = parser;
                DstFilePath = dstFilePath;
                _dstWordToValuesLocationContainer = new Dictionary<Word, ValuesLocationContainer>();

                _inited = false;
                _hasComplexWords = false;
                _changed = false;

                foreach (var dstWord in dstWords)
                {
                    if (dstWord.IsComplex())
                    {
                        _hasComplexWords = true;
                    }

                    Debug.Assert(!_dstWordToValuesLocationContainer.ContainsKey(dstWord), $"inner container already contains this dstWord={dstWord}, dstFilePath={dstFilePath}");
                    _dstWordToValuesLocationContainer[dstWord] = new ValuesLocationContainer(dstFilePath);
                }
            }

            internal JumpLocation? FindDestinationLocation(Word dstWord, string value)
            {
                var dstValuesLocationContainer = _dstWordToValuesLocationContainer[dstWord];
                return dstValuesLocationContainer.FindDefinitionByValue(value);
            }

            private void OnDstValueFound(Word dstWord, int lineNumber, string value)
            {
                _dstWordToValuesLocationContainer[dstWord].PutOrReplace(value, lineNumber);
            }

            public void InitIfNeeded(bool cacheEnabled)
            {
                if (!cacheEnabled)
                {
                    ClearIfChanged();
                }

                if (_inited) return;

                Logger.Info($"Init container for dstFile={DstFilePath}");

                if (!File.Exists(DstFilePath))
                {
                    Logger.Error($"dstFile={DstFilePath} not exist");
                    return;
                }

                try
                {
                    _parser.ParseValidDocument(DstFilePath, _dstWordToValuesLocationContainer.Keys, OnDstValueFound);
                }
                catch (Exception)
                {
                    if (_hasComplexWords)
                    {
                        Logger.Error($"cannot parse invalid json file: {DstFilePath}");
                    }

                    _parser.ParseInvalidDocument(DstFilePath, _dstWordToValuesLocationContainer.Keys, OnDstValueFound);
                }

                _inited = true;
            }

            private void ClearIfChanged()
            {
                if (!_changed) return;

                Logger.Info($"Clear container for dstFile={DstFilePath}");

                _inited = false;
                foreach (var entry in _dstWordToValuesLocationContainer)
                {
                    entry.Value.Clear();
                }

                _changed = false;
            }

            internal void OnContentChanged()
            {
                if (_changed) return;

                Logger.Info($"Container for dstFile={DstFilePath} mark changed");
                _changed = true;
            }
        }

        private class ValuesLocationContainer
        {
            private readonly string _dstFilePath;
            private readonly IDictionary<string, int> _valueToLineMap; // value ->  jumpLine

            internal ValuesLocationContainer(string dstFilePath)
            {
                _dstFilePath = dstFilePath;
                _valueToLineMap = new Dictionary<string, int>();
            }

            internal JumpLocation? FindDefinitionByValue(string value)
            {
                return _valueToLineMap.TryGetValue(value, out int line)
                    ? new JumpLocation(_dstFilePath, line)
                    : null;
            }

            internal void PutOrReplace(string value, int lineNumber)
            {
                _valueToLineMap[value] = lineNumber;
            }

            internal void Clear()
            {
                _valueToLineMap.Clear();
            }
        }
    }
}