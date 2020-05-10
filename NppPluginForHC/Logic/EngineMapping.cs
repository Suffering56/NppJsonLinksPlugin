using System;
using System.Collections.Generic;

namespace NppPluginForHC.Logic
{
    public class EngineMapping
    {
        public EngineMapping()
        {
            
        }


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