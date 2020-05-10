using System;
using System.Collections.Generic;

namespace NppPluginForHC.Logic
{
    public class EngineMapping
    {
        public EngineMapping(List<string> rawMappingList)
        {
            foreach (string str in rawMappingList)
            {
                string[] split = str.Split(new string[] {"==>"}, StringSplitOptions.None);

                string srcString = split[0];
                string dstString = split[1];

                string[] srcSplit = srcString.Split(':');
                string srcFileName = srcSplit[0];

                // Word srcWord = new Word(srcSplit);
            }
        }

       
        // D:/projects/shelter/gd_data/festivalGoods.json:[]:extension:rewardPackId==>D:/projects/shelter/gd_data/rewardPacks.json:[]:id
        // D:/projects/shelter/gd_data/festivalGoods.json:[]:rewardPackId==>D:/projects/shelter/gd_data/rewardPacks.json:[]:id


        private Dictionary<string, FileMappingData> fileMappingDataMap; // sourceFile -> MappingData

        class FileMappingData
        {
            private string srcFileName;
            private Dictionary<string, DestinationData> destinationDataMap; // dstFileName -> Data
        }

        class DestinationData
        {
            private string dstFileName;
            private ISet<Word> definitionWords;
        }
    }
}