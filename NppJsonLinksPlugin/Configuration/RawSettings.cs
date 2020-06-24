using System.Collections.Generic;
using Newtonsoft.Json;

namespace NppJsonLinksPlugin.Configuration
{
    [JsonObject]
    public class RawSettings
    {
        [JsonProperty(PropertyName = "highlightingEnabled")]
        public bool HighlightingEnabled = true;

        [JsonProperty(PropertyName = "soundEnabled")]
        public bool SoundEnabled = true;

        [JsonProperty(PropertyName = "jumpToLineDelay")]
        public int JumpToLineDelay = AppConstants.DEFAULT_JUMP_TO_LINE_DELAY;

        [JsonProperty(PropertyName = "mappingDefaultFilePath")]
        public string MappingDefaultFilePath = null;

        [JsonProperty(PropertyName = "mapping")]
        public IList<RawMappingItem> Mapping;

        [JsonObject]
        public class RawMappingItem
        {
            [JsonProperty(PropertyName = "description")]
            public string Description;

            [JsonProperty(PropertyName = "src", Required = Required.Always)]
            public IList<RawLocation> Src;

            [JsonProperty(PropertyName = "dst", Required = Required.Always)]
            public RawLocation Dst;

            [JsonObject]
            public class RawLocation
            {
                [JsonProperty(PropertyName = "overrideFilePath")]
                public string OverrideFilePath;

                [JsonProperty(PropertyName = "fileName", Required = Required.Always)]
                public string FileName;

                [JsonProperty(PropertyName = "ignoredFileNames")]
                public List<string> IgnoredFileNames = new List<string>();

                [JsonProperty(PropertyName = "word", Required = Required.Always)]
                public string Word;

                [JsonProperty(PropertyName = "fileNameRegexpEnabled")]
                public bool FileNameRegexpEnabled = false;

                [JsonProperty(PropertyName = "wordRegexpEnabled")]
                public bool WordRegexpEnabled = false;
                
                [JsonProperty(PropertyName = "order")]
                public int Order = AppConstants.DEFAULT_DST_ORDER;
            }
        }
    }
}