#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NppPluginForHC.Logic
{
    public class DefinitionSearchEngine
    {
        public delegate SearchContext ContextProvider();

        private readonly IDictionary<string, FileDestinationsContainer> _sourceFilePathToContainerMap; // sourceFilePath -> FileDestinationsContainer
        private string _currentFilePath = null;

        public DefinitionSearchEngine()
        {
            _sourceFilePathToContainerMap = new Dictionary<string, FileDestinationsContainer>();
        }

        public void OnSwitchContext(string currentFilePath)
        {
            if (currentFilePath == _currentFilePath) return;

            Logger.Info($"OnSwitchContext[changed={currentFilePath != _currentFilePath}]: <{_currentFilePath}> to <{currentFilePath}>");
            _currentFilePath = currentFilePath;

            try
            {
                var fileDestinations = GetOrTryCreateFileDestinations(currentFilePath);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }

            // var settings = Main.Settings;
        }

        public JumpLocation? FindDefinitionLocation(string selectedWord, ContextProvider contextProvider)
        {
            FileDestinationsContainer fileContainer = GetCurrentFileDestinations();
            return fileContainer?.FindDestination(selectedWord, contextProvider);
        }

        private FileDestinationsContainer? GetCurrentFileDestinations()
        {
            return _sourceFilePathToContainerMap.TryGetValue(_currentFilePath, out var fileContainer)
                ? fileContainer
                : null;
        }

        private FileDestinationsContainer? GetOrTryCreateFileDestinations(string currentFilePath)
        {
            if (_sourceFilePathToContainerMap.TryGetValue(currentFilePath, out var fileContainer))
            {
                return fileContainer;
            }

            var mappingItems = Main.Settings.Mapping;
            foreach (var mappingItem in mappingItems)
            {
                if (currentFilePath != mappingItem.Src.FilePath) continue;

                if (fileContainer == null)
                {
                    fileContainer = new FileDestinationsContainer(currentFilePath);
                    _sourceFilePathToContainerMap[currentFilePath] = fileContainer;
                }

                fileContainer.InitMappingLazy(mappingItem);
            }

            return fileContainer;
        }

        private class FileDestinationsContainer
        {
            private readonly string _sourceFileName;
            private readonly IDictionary<Word, WordDestinationsContainer> _srcWordToContainerMap; // srcWord -> WordDestinationContainer
            private bool _hasComplexWords;

            internal FileDestinationsContainer(string sourceFileName)
            {
                _sourceFileName = sourceFileName;
                _hasComplexWords = false;
                _srcWordToContainerMap = new Dictionary<Word, WordDestinationsContainer>();
            }

            internal void InitMappingLazy(Settings.MappingItem mappingItem)
            {
                Word srcWord = mappingItem.Src.Word;
                Debug.Assert(!_srcWordToContainerMap.ContainsKey(srcWord), $"word already defined for srcFile={_sourceFileName}, word={srcWord}");

                _srcWordToContainerMap[srcWord] = new WordDestinationsContainer(mappingItem);
                if (srcWord.IsComplex())
                {
                    _hasComplexWords = true;
                }
            }

            private void InitAllWordsContainers()
            {
                if (_hasComplexWords)
                {
                    InitSubContainersByComplexWords();
                }
                else
                {
                    InitSubContainersBySimpleWords();
                }
            }

            private void InitSubContainersBySimpleWords()
            {
                
                //нужно найти все маппинги у которых mapping.dst.filePath == dstFilePath
                
                using (StreamReader sr = new StreamReader("F:/mobSettings.json"))
                    // using (StreamReader sr = new StreamReader("D:/projects/shelter/gd_data/mobSettings.json"))
                    // using (StreamReader sr = new StreamReader(dstFilePath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        // if (line.Contains(dstWord.WordString))
                        if (line.Contains("LID_mobs_warrior_tank"))
                        {
                            Logger.Info($"line={line}");
                        }
                    }
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

            internal JumpLocation? FindDestination(string srcWordString, ContextProvider contextProvider)
            {
                var srcWord = GetWord(srcWordString, contextProvider);
                if (srcWord == null) return null; // инициализация не нужна для слов, не являющихся ссылками
                InitAllWordsContainers();

                var wordValuesContainer = _srcWordToContainerMap[srcWord];
                var searchContext = contextProvider.Invoke();
                var gateway = searchContext.Gateway;

                object value = 123;

                return wordValuesContainer.FindDefinitionByValue(value);
            }


            private Word? GetWord(string wordString, ContextProvider contextProvider)
            {
                foreach (var word in _srcWordToContainerMap.Keys)
                {
                    if (word.WordString == wordString)
                    {
                        if (!word.IsComplex()) return word;

                        var searchContext = contextProvider();
                        throw new NotImplementedException("//TODO: support complex words");
                    }
                }

                return null;
            }
        }

        private class WordDestinationsContainer
        {
            private readonly IDictionary<object, JumpLocation> _destinationByWordValueMap; // word.value -> destination
            internal Settings.MappingItem? MappingItem;

            internal WordDestinationsContainer(Settings.MappingItem mappingItem)
            {
                _destinationByWordValueMap = new Dictionary<object, JumpLocation>();
                MappingItem = mappingItem;
            }


            public JumpLocation? FindDefinitionByValue(object value)
            {
                return _destinationByWordValueMap.TryGetValue(value, out JumpLocation jumpLocation)
                    ? jumpLocation
                    : null;
            }

            internal void Init()
            {
                Debug.Assert(MappingItem != null, $"inner container already inited for word: {MappingItem}");

                var dstWord = MappingItem.Dst.Word;
                var dstFilePath = MappingItem.Dst.FilePath;

                MappingItem = null;
            }
        }
    }
}