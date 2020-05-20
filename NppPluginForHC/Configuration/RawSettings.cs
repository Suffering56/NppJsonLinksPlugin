using System.Collections.Generic;
using Newtonsoft.Json;
using NppPluginForHC.Core;

namespace NppPluginForHC.Configuration
{
    [JsonObject]
    public class RawSettings
    {
        [JsonProperty(PropertyName = "loggerMode")]
        public Logger.Mode LoggerMode = AppConstants.DefaultLoggerMode;

        [JsonProperty(PropertyName = "logPathPrefix")]
        public string LogPathPrefix = null;

        [JsonProperty(PropertyName = "cacheEnabled")]
        public bool CacheEnabled = false;

        [JsonProperty(PropertyName = "soundEnabled")]
        public bool SoundEnabled = true;

        [JsonProperty(PropertyName = "jumpToLineDelay")]
        public int JumpToLineDelay = AppConstants.DefaultJumpToLineDelay;

        [JsonProperty(PropertyName = "mappingFilePathPrefix")]
        public string MappingFilePathPrefix;

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

        public class RawLoggerSettings
        {
        }
    }
}