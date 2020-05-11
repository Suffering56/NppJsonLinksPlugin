using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NppPluginForHC.Logic
{
    [JsonObject]
    public class RawSettings
    {
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

                [JsonConverter(typeof(WordConverter))] [JsonProperty(PropertyName = "word", Required = Required.Always)]
                public Word Word;

                [JsonProperty(PropertyName = "filePathPrefixDisabled")]
                public bool FilePathPrefixDisabled = false;
            }
        }
    }

    internal class WordConverter : JsonConverter<Word>
    {
        public override Word ReadJson(JsonReader reader, Type objectType, Word existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var wordStr = reader.Value;
            if (wordStr == null) return null;

            //TODO: existingValue? hasExistingValue? what is it?
            return Word.Parse(Convert.ToString(wordStr));
        }

        public override void WriteJson(JsonWriter writer, Word value, JsonSerializer serializer)
        {
            //TODO: NPE?
            writer.WriteValue(value.ToString());

            throw new NotImplementedException();
        }
    }
}