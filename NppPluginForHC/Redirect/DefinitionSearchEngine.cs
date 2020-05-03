using System;

namespace NppPluginForHC.Redirect
{
    public class DefinitionSearchEngine
    {
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
            Log.Out($"OnSwitchContext[changed={currentFilePath != _currentFilePath}]: <{_currentFilePath}> to <{currentFilePath}>");
            if (currentFilePath != _currentFilePath)
            {
                //TODO
            }

            _currentFilePath = currentFilePath;
        }
    }
}