using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace NppPluginForHC.Logic
{
    public static class SettingsParser
    {
        public static Settings Parse(string settingsFilePath)
        {
            var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingsFilePath));
            Validate(settings);
            settings.SpecifyMapping();
            return settings;
        }

        private static void Validate(Settings settings)
        {
            //TODO: как минимум проверить маппинг на отсутствие дубликатов
        }
    }

    [JsonObject]
    public class Settings
    {
        [JsonProperty(PropertyName = "mappingFilePathPrefix")]
        internal readonly string MappingFilePathPrefix;

        [JsonProperty(PropertyName = "mapping")]
        public readonly IList<MappingItem> Mapping;

        internal void SpecifyMapping()
        {
            if (string.IsNullOrEmpty(MappingFilePathPrefix)) return;

            foreach (var mappingItem in Mapping)
            {
                mappingItem.Src.SpecifyMapping(MappingFilePathPrefix);
                mappingItem.Dst.SpecifyMapping(MappingFilePathPrefix);
            }
        }

        [JsonObject]
        public class MappingItem
        {
            [JsonProperty(PropertyName = "description")]
            public readonly string Description;

            [JsonProperty(PropertyName = "src", Required = Required.Always)]
            public readonly Location Src;

            [JsonProperty(PropertyName = "dst", Required = Required.Always)]
            public readonly Location Dst;

            [JsonObject]
            public class Location
            {
                [JsonProperty(PropertyName = "filePath", Required = Required.Always)]
                private readonly string _filePath;

                [JsonConverter(typeof(WordConverter))] [JsonProperty(PropertyName = "word", Required = Required.Always)]
                public readonly Word Word;

                [JsonProperty(PropertyName = "filePathPrefixDisabled")]
                private readonly bool _filePathPrefixDisabled = false;

                [JsonIgnore] private string _mappingFilePathPrefix = null;

                internal void SpecifyMapping(string mappingFilePathPrefix)
                {
                    //TODO: пока не понял как тут работают иннер классы
                    if (!_filePathPrefixDisabled)
                    {
                        _mappingFilePathPrefix = mappingFilePathPrefix;
                    }
                }

                public string FilePath => _mappingFilePathPrefix != null
                    ? NormalizePath(_mappingFilePathPrefix + _filePath)
                    : NormalizePath(_filePath);

                private string NormalizePath(string path)
                {
                    return Path.GetFullPath(path);
                }
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