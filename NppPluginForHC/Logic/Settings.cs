using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NppPluginForHC.Logic
{
    public static class SettingsParser
    {
        public static String str = str = "{\n" +
                                         // "  \"someSettings\": {\n" +
                                         // "    \"example1\": \"D:/projects/shelter/gd_data/festivalGoods.json:[]: extension: rewardPackId==>D: /projects/shelter/gd_data/rewardPacks.json:[]: id\",\n" +
                                         // "    \"example2\": \"D:/projects/shelter/gd_data/festivalGoods.json:[]: rewardPackId==>D: /projects/shelter/gd_data/rewardPacks.json:[]: id\"\n" +
                                         // "  },\n" +
                                         "  \"mappingFolderPrefix\": \"D:/projects/shelter/gd_data/\",\n" +
                                         "  \"mapping\": [\n" +
                                         "    {\n" +
                                         "      \"description\": \"\",\n" +
                                         "      \"src\": {\n" +
                                         "        \"fileName\": \"festivalGoods.json\",\n" +
                                         "        \"word\": \"rewardPackId<<extension\"\n" +
                                         "      },\n" +
                                         "      \"dst\": {\n" +
                                         "        \"fileName\": \"festivalGoods.json\",\n" +
                                         "        \"word\": \"id<<root\"\n" +
                                         "      }\n" +
                                         "    },\n" +
                                         "    {\n" +
                                         "      \"description\": \"\",\n" +
                                         "      \"src\": {\n" +
                                         "        \"fileName\": \"festivalGoods.json\",\n" +
                                         "        \"word\": \"rewardPackId\"\n" +
                                         "      },\n" +
                                         "      \"dst\": {\n" +
                                         "        \"fileName\": \"festivalGoods.json\",\n" +
                                         "        \"word\": \"id<<root\"\n" +
                                         "      }\n" +
                                         "    }\n" +
                                         "  ]\n" +
                                         "}";


        public static Settings Parse(string settingsFilePath)
        {
            return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingsFilePath));
        }
    }


    [JsonObject]
    public class Settings
    {
        [JsonProperty] public readonly string mappingFolderPrefix;

        [JsonProperty] public readonly IList<Mapping> mapping;

        [JsonObject]
        public struct Mapping
        {
            [JsonProperty] public readonly String description;

            [JsonProperty(Required = Required.Always)]
            public readonly Location src;

            [JsonProperty(Required = Required.Always)]
            public readonly Location dst;
        }

        [JsonObject]
        public struct Location
        {
            [JsonProperty(Required = Required.Always)]
            public readonly string fileName;

            [JsonProperty(Required = Required.Always)] [JsonConverter(typeof(WordConverter))]
            public readonly Word word;
        }
    }

    internal class WordConverter : JsonConverter<Word>
    {
        public override Word ReadJson(JsonReader reader, Type objectType, Word existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var wordStr = reader.Value;

            if (wordStr == null)
            {
                return null;
            }

            //TODO: existingValue? hasExistingValue? what is it?
            return Word.Parse(Convert.ToString(wordStr));
        }

        public override void WriteJson(JsonWriter writer, Word value, JsonSerializer serializer)
        {
            throw new NotImplementedException();

            //TODO: NPE?
            writer.WriteValue(value.ToString());
        }
    }
}