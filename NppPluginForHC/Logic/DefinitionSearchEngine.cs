using System;
using System.Collections.Generic;
using System.IO;

namespace NppPluginForHC.Logic
{
    public class DefinitionSearchEngine
    {
        private Dictionary<string, FileLinksContainer> linksMap; // filename -> FileLinksContainer

        private string _currentFilePath = null;

        public JumpLocation? FindDefinitionLocation(string selectedWord, int currentLine)
        {
            if (selectedWord == "rewardPackId")
            {
                return new JumpLocation("D:/projects/shelter/gd_data/abilities.json", 4720);
            }

            return null;
        }

        public void OnSwitchContext(string currentFilePath)
        {
            Logger.Out($"OnSwitchContext[changed={currentFilePath != _currentFilePath}]: <{_currentFilePath}> to <{currentFilePath}>");
            if (currentFilePath != _currentFilePath)
            {
                //TODO

                var settings = Main.Settings;
            }

            _currentFilePath = currentFilePath;
        }

        private class FileLinksContainer
        {
            private Dictionary<Word, WordDestinationContainer> wordDestinationContainersMap; // word -> WordDestinationContainer
        }

        private class WordDestinationContainer
        {
            private Dictionary<object, JumpLocation> destinationByValueMap; // value -> destination
        }
    }
}