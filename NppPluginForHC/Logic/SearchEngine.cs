#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NppPluginForHC.Core;
using static NppPluginForHC.Logic.Settings;

namespace NppPluginForHC.Logic
{
    public class DefinitionSearchEngine
    {
        //TODO: override MappingItem equals and hashCode 
        private readonly IExtendedDictionary<MappingItem, DstFileContainer> _mappingToFileContainerMap; // dstFilePath -> DstFileContainer
        private readonly ISet<string> _availableSrcWords;

        private string _currentFilePath = null;
        private bool _cacheEnabled;

        public DefinitionSearchEngine()
        {
            _mappingToFileContainerMap = new ExtendedDictionary<MappingItem, DstFileContainer>();
            _availableSrcWords = new HashSet<string>();
        }

        public void Init(Settings settings, string currentFilePath)
        {
            _cacheEnabled = settings.CacheEnabled;
            var dstFilePathToDstWordsMap = new ExtendedDictionary<string, ISet<Word>>(); // dstFilePath -> ISet<dstWord>

            foreach (var mappingItem in settings.Mapping)
            {
                var dst = mappingItem.Dst;
                var dstFilePath = dst.FilePath;

                var fileDstWords = dstFilePathToDstWordsMap.ComputeIfAbsent(dstFilePath, key => new HashSet<Word>());
                fileDstWords.Add(dst.Word);
            }

            foreach (var mappingItem in settings.Mapping)
            {
                var dstFilePath = mappingItem.Dst.FilePath;
                var supportedWords = dstFilePathToDstWordsMap[dstFilePath];

                Debug.Assert(!_mappingToFileContainerMap.ContainsKey(mappingItem), $"outer container already contains mappingItem={mappingItem}");
                _mappingToFileContainerMap[mappingItem] = new DstFileContainer(dstFilePath, supportedWords);

                // для быстрой проверки
                _availableSrcWords.Add(mappingItem.Src.Word.WordString);
            }

            SwitchContext(currentFilePath);
        }

        public void SwitchContext(string currentFilePath)
        {
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

        public JumpLocation? FindDefinitionLocation(string selectedWordString, SearchContextProvider searchContextProvider)
        {
            var mappingItem = GetMappingItem(selectedWordString, searchContextProvider);
            if (mappingItem == null) return null;

            var dstFileContainer = _mappingToFileContainerMap[mappingItem];


            if (!_cacheEnabled)
            {
                dstFileContainer.ClearIfChanged();
            }

            dstFileContainer.InitIfNeeded();

            string tokenValue = searchContextProvider().GetTokenValue(selectedWordString);
            if (tokenValue == null) return null;

            return dstFileContainer.FindDestinationLocation(mappingItem.Dst.Word, tokenValue);
        }

        private MappingItem? GetMappingItem(string selectedWordString, SearchContextProvider searchContextProvider)
        {
            if (!_availableSrcWords.Contains(selectedWordString)) return null;

            foreach (var mappingItem in _mappingToFileContainerMap.Keys)
            {
                if (mappingItem.Src.FilePath != _currentFilePath) continue;

                var srcWord = mappingItem.Src.Word;
                if (srcWord.WordString != selectedWordString) continue;

                if (!srcWord.IsComplex()) return mappingItem;

                var searchContext = searchContextProvider();

                if (searchContext.IsSelectedWordEqualsWith(srcWord)) return mappingItem;
            }

            return null;
        }

        private DstFileContainer GetContainerByDstFilePath(string dstFilePath)
        {
            return _mappingToFileContainerMap.Values.FirstOrDefault(dstFileContainer => dstFileContainer.DstFilePath == dstFilePath);
        }

        private class DstFileContainer
        {
            internal string DstFilePath { get; }
            private readonly IDictionary<Word, ValuesLocationContainer> _dstWordToValuesLocationContainer; // dstWord -> ValuesLocationContainer
            private readonly bool _hasComplexWords;
            private bool _inited;
            private bool _changed;

            internal DstFileContainer(string dstFilePath, ISet<Word> dstWords)
            {
                _inited = false;
                DstFilePath = dstFilePath;
                _dstWordToValuesLocationContainer = new Dictionary<Word, ValuesLocationContainer>();
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

            public void InitIfNeeded()
            {
                if (_inited) return;

                if (!File.Exists(DstFilePath))
                {
                    Logger.Error($"dstFile={DstFilePath} not exist");
                    return;
                }

                var parser = new SearchEngineJsonParser(_dstWordToValuesLocationContainer);
                try
                {
                    parser.TryParseValidJson(DstFilePath);
                }
                catch (Exception)
                {
                    if (_hasComplexWords)
                    {
                        Logger.Error($"cannot parse invalid json file: {DstFilePath}");
                    }

                    parser.ParseInvalidJson(DstFilePath);
                }

                _inited = true;
            }

            internal void ClearIfChanged()
            {
                if (!_changed) return;

                _inited = false;
                foreach (var entry in _dstWordToValuesLocationContainer)
                {
                    entry.Value.Clear();
                }

                _changed = false;
            }

            internal void OnContentChanged()
            {
                _changed = true;
            }
        }

        public class ValuesLocationContainer
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