using System.Collections.Generic;
using Newtonsoft.Json;

namespace NppPluginForHC.Configuration
{
    [JsonObject]
    public class RawSettings
    {
        [JsonProperty(PropertyName = "mappingFilePathPrefix")]
        public string MappingFilePathPrefix;

        [JsonProperty(PropertyName = "cacheEnabled")]
        public bool CacheEnabled = false;

        [JsonProperty(PropertyName = "jumpToLineDelay")]
        public int JumpToLineDelay = Settings.DefaultJumpToLineDelay;

        [JsonProperty(PropertyName = "mapping")]
        public IList<RawMappingItem> Mapping;

        [JsonObject]
        public class RawMappingItem
        {
            [JsonProperty(PropertyName = "description")]
            public string Description;

            [JsonProperty(PropertyName = "src", Required = Required.Always)]
            public RawLocation Src;

            [JsonProperty(PropertyName = "dst", Required = Required.Always)]
            public RawLocation Dst;

            [JsonObject]
            public class RawLocation
            {
                [JsonProperty(PropertyName = "filePath", Required = Required.Always)]
                public string FilePath;

                [JsonProperty(PropertyName = "word", Required = Required.Always)]
                public string Word;

                [JsonProperty(PropertyName = "filePathPrefixEnabled")]
                public bool FilePathPrefixEnabled = true;
            }
        }
    }
}