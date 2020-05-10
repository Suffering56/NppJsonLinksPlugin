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

        public DefinitionSearchEngine()
        {
            _mappingToFileContainerMap = new ExtendedDictionary<MappingItem, DstFileContainer>();
            _availableSrcWords = new HashSet<string>();
        }

        public void Init(Settings settings, string currentFilePath)
        {
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

            //TODO: NYI
        }

        public JumpLocation? FindDefinitionLocation(string selectedWordString, SearchContextProvider searchContextProvider)
        {
            var mappingItem = GetMappingItem(selectedWordString, searchContextProvider);
            var dstFileContainer = _mappingToFileContainerMap[mappingItem];

            dstFileContainer.InitIfNeeded();

            return dstFileContainer.FindDestinationLocation(mappingItem.Dst.Word, searchContextProvider().GetTokenValue());
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
                throw new NotImplementedException("//TODO: support complex words");
            }

            return null;
        }

        private class DstFileContainer
        {
            private readonly string _dstFilePath;
            private readonly IDictionary<Word, ValuesLocationContainer> _dstWordToValuesLocationContainer; // dstWord -> ValuesLocationContainer
            private readonly bool _hasComplexWords;
            private bool _inited;

            internal DstFileContainer(string dstFilePath, ISet<Word> dstWords)
            {
                _inited = false;
                _dstFilePath = dstFilePath;
                _dstWordToValuesLocationContainer = new Dictionary<Word, ValuesLocationContainer>();
                _hasComplexWords = false;

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

            internal JumpLocation? FindDestinationLocation(Word dstWord, object value)
            {
                var dstValuesLocationContainer = _dstWordToValuesLocationContainer[dstWord];
                return dstValuesLocationContainer.FindDefinitionByValue(value);
            }

            public void InitIfNeeded()
            {
                if (_inited) return;

                if (!File.Exists(_dstFilePath))
                {
                    Logger.Error($"dstFile={_dstFilePath} not exist");
                    return;
                }

                if (_hasComplexWords)
                {
                    InitSubContainersByComplexWords();
                }
                else
                {
                    InitSubContainersBySimpleWords();
                }

                _inited = true;
            }

            private void InitSubContainersBySimpleWords()
            {
                using StreamReader sr = new StreamReader(_dstFilePath);
                int lineNumber = 0;
                string lineText;
                while ((lineText = sr.ReadLine()) != null)
                {
                    foreach (var entry in _dstWordToValuesLocationContainer)
                    {
                        var dstWordString = entry.Key.WordString;
                        var valuesContainer = entry.Value;

                        if (lineText.Contains(dstWordString))
                        {
                            object value = "MOBRANGER"; // TODO extract value
                            // valuesContainer.PutOrReplace(value, lineNumber);
                            valuesContainer.PutOrReplace(value, 27);
                        }
                    }

                    lineNumber++;
                }
            }

            private void InitSubContainersByComplexWords()
            {
                throw new NotImplementedException();

                string json = @"{
                               'CPU': 'Intel',
                               'PSU': '500W',
                               'Drives': [
                                 'DVD read/writer'
                                 /*(broken)*/,
                                 '500 gigabyte hard drive',
                                 '200 gigabyte hard drive'
                               ]
                            }";

                JsonTextReader reader = new JsonTextReader(new StringReader(json));
                while (reader.Read())
                {
                    if (reader.Value != null)
                    {
                        Logger.Info($"Token: {reader.TokenType}, Value: {reader.Value}");
                    }
                    else
                    {
                        Logger.Info($"Token: {reader.TokenType}");
                    }
                }
            }
        }

        private class ValuesLocationContainer
        {
            private readonly string _dstFilePath;
            private readonly IDictionary<object, int> _valueToLineMap; // value -> JumpLocation

            internal ValuesLocationContainer(string dstFilePath)
            {
                _dstFilePath = dstFilePath;
                _valueToLineMap = new Dictionary<object, int>();
            }

            internal JumpLocation? FindDefinitionByValue(object value)
            {
                return _valueToLineMap.TryGetValue(value, out int line)
                    ? new JumpLocation(_dstFilePath, line)
                    : null;
            }

            internal void PutOrReplace(object value, int lineNumber)
            {
                _valueToLineMap[value] = lineNumber;
            }
        }
    }
}