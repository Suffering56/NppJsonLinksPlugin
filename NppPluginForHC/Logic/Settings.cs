using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using static NppPluginForHC.Logic.RawSettings;

namespace NppPluginForHC.Logic
{
    public static class SettingsParser
    {
        public static Settings Parse(string settingsFilePath)
        {
            var rawSettings = JsonConvert.DeserializeObject<RawSettings>(File.ReadAllText(settingsFilePath));
            Settings settings = ConvertRawSettings(rawSettings);
            Validate(settings);
            return settings;
        }

        private static Settings ConvertRawSettings(RawSettings rawSettings)
        {
            var mappingItems = rawSettings.Mapping
                .Select(rawMappingItem => new Settings.MappingItem
                {
                    Src = ConvertRawLocation(rawMappingItem.Src, rawSettings.MappingFilePathPrefix),
                    Dst = ConvertRawLocation(rawMappingItem.Dst, rawSettings.MappingFilePathPrefix)
                })
                .ToList();

            return new Settings
            {
                MappingFilePathPrefix = rawSettings.MappingFilePathPrefix,
                Mapping = mappingItems
            };
        }

        private static Settings.MappingItem.Location ConvertRawLocation(RawMappingItem.RawLocation location, string mappingFilePathPrefix)
        {
            return new Settings.MappingItem.Location
            {
                Word = location.Word,
                FilePath = location.FilePathPrefixDisabled
                    ? Path.GetFullPath(location.FilePath)
                    : Path.GetFullPath(mappingFilePathPrefix + location.FilePath)
            };
        }

        private static void Validate(Settings settings)
        {
            //TODO: как минимум проверить маппинг на отсутствие дубликатов
        }
    }
    
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public class Settings
    {
        public string MappingFilePathPrefix { get; internal set; }

        public IEnumerable<MappingItem> Mapping { get; internal set; }

        public class MappingItem
        {
            public Location Src { get; internal set; }
            public Location Dst { get; internal set; }

            private bool Equals(MappingItem other)
            {
                return Equals(Src, other.Src) && Equals(Dst, other.Dst);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((MappingItem) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Src != null ? Src.GetHashCode() : 0) * 397) ^ (Dst != null ? Dst.GetHashCode() : 0);
                }
            }

            public class Location
            {
                public string FilePath { get; internal set; }
                public Word Word { get; internal set; }

                private bool Equals(Location other)
                {
                    return FilePath == other.FilePath && Equals(Word, other.Word);
                }

                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(null, obj)) return false;
                    if (ReferenceEquals(this, obj)) return true;
                    if (obj.GetType() != this.GetType()) return false;
                    return Equals((Location) obj);
                }

                public override int GetHashCode()
                {
                    unchecked
                    {
                        return ((FilePath != null ? FilePath.GetHashCode() : 0) * 397) ^ (Word != null ? Word.GetHashCode() : 0);
                    }
                }
            }
        }
    }
}