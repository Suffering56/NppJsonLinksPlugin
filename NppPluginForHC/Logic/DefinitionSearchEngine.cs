#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            if (mappingItem == null) return null;

            var dstFileContainer = _mappingToFileContainerMap[mappingItem];

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

            internal JumpLocation? FindDestinationLocation(Word dstWord, string value)
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

                try
                {
                    InitSubContainersByJsonReader();
                }
                catch (Exception)
                {
                    InitSubContainersByStringReader();
                }

                _inited = true;
            }

            private void InitSubContainersByJsonReader()
            {
                string expectedWord = null;

                using JsonTextReader reader = new JsonTextReader(new StreamReader(_dstFilePath));
                while (reader.Read())
                {
                    if (reader.Value == null) continue;

                    foreach (var entry in _dstWordToValuesLocationContainer)
                    {
                        var dstWord = entry.Key;
                        var valuesContainer = entry.Value;
                        // var dstWordString = entry.Key.WordString;
                        // var tokenType = reader.TokenType;

                        if (!dstWord.IsComplex())
                        {
                            ParseSimpleWord(reader, valuesContainer, dstWord, ref expectedWord);
                        }
                    }
                }
            }

            private static void ParseSimpleWord(JsonTextReader reader, ValuesLocationContainer valuesContainer, Word dstWord, ref string expectedWord)
            {
                var tokenType = reader.TokenType;

                //ожидаем property
                if (tokenType == JsonToken.PropertyName) // TODO: or StartToken/EndToken/etc..
                {
                    expectedWord = null;

                    if (dstWord.WordString == reader.Value.ToString())
                    {
                        expectedWord = dstWord.WordString;
                    }

                    return;
                }

                if (expectedWord != dstWord.WordString) return;

                //ожидаем value
                string valueString = reader.Value.ToString();
                switch (tokenType)
                {
                    case JsonToken.Boolean:
                        valueString = valueString.ToLower();
                        break;

                    case JsonToken.Float:
                        valueString = valueString.Replace(',', '.');
                        break;

                    case JsonToken.Integer:
                    case JsonToken.String:
                        break;

                    case JsonToken.None:
                    case JsonToken.StartObject:
                    case JsonToken.StartArray:
                    case JsonToken.StartConstructor:
                    case JsonToken.PropertyName:
                    case JsonToken.Comment:
                    case JsonToken.Raw:
                    case JsonToken.Null:
                    case JsonToken.Undefined:
                    case JsonToken.EndObject:
                    case JsonToken.EndArray:
                    case JsonToken.EndConstructor:
                    case JsonToken.Date:
                    case JsonToken.Bytes:
                    default:
                        return;
                }

                valuesContainer.PutOrReplace(valueString, reader.LineNumber);
                expectedWord = null;
            }

            private void InitSubContainersByStringReader()
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

                        if (!lineText.Contains($"\"{dstWordString}\"")) continue;

                        string value = Utils.ExtractTokenValueByLine(lineText, dstWordString);
                        if (value != null)
                        {
                            valuesContainer.PutOrReplace(value, lineNumber);
                        }
                    }

                    lineNumber++;
                }

                return;
                throw new NotImplementedException();

                // Token: StartObject
                // Token: PropertyName, Value: CPU
                // Token: String, Value: Intel
                // Token: PropertyName, Value: PSU
                // Token: String, Value: 500W
                // Token: PropertyName, Value: Drives
                // Token: StartArray
                // Token: String, Value: DVD read/writer
                // Token: Comment, Value: (broken)
                //     Token: String, Value: 500 gigabyte hard drive
                // Token: String, Value: 200 gigabyte hard drive
                // Token: EndArray
                // Token: EndObject


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
            private readonly IDictionary<string, int> _valueToLineMap; // value -> JumpLocation

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
        }
    }
}