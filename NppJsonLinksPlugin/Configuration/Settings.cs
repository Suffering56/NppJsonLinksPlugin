using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NppJsonLinksPlugin.Core;
using NppJsonLinksPlugin.Logic;

namespace NppJsonLinksPlugin.Configuration
{
    public static class SettingsParser
    {
        public static Settings Parse(string settingsFilePath)
        {
            var rawSettings = JsonConvert.DeserializeObject<RawSettings>(File.ReadAllText(settingsFilePath));
            Settings settings = ConvertRawSettings(rawSettings);
            
            Logger.SetMode(settings.LoggerMode, settings.LogPathPrefix);
            
            try
            {
                Validate(settings);
            }
            catch (Exception e)
            {
                Logger.ErrorMsgBox(e.Message);
                Main.IsPluginEnabled = false;
            }

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
                LoggerMode = rawSettings.LoggerMode,
                LogPathPrefix = rawSettings.LogPathPrefix,
                CacheEnabled = rawSettings.CacheEnabled,
                SoundEnabled = rawSettings.SoundEnabled,
                JumpToLineDelay = rawSettings.JumpToLineDelay,
                MappingFilePathPrefix = rawSettings.MappingFilePathPrefix,
                Mapping = mappingItems
            };
        }

        private static Settings.MappingItem.Location ConvertRawLocation(RawSettings.RawMappingItem.RawLocation location, string mappingFilePathPrefix)
        {
            return new Settings.MappingItem.Location
            {
                Word = Word.Parse(location.Word),
                FilePath = location.FilePathPrefixEnabled
                    ? Path.GetFullPath(mappingFilePathPrefix + location.FilePath)
                    : Path.GetFullPath(location.FilePath)
            };
        }

        private static void Validate(Settings settings)
        {
        }
    }

    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public class Settings
    {
        public Logger.Mode LoggerMode { get; internal set; }

        public string LogPathPrefix { get; internal set; }

        public bool CacheEnabled { get; internal set; }

        public bool SoundEnabled { get; internal set; }

        public int JumpToLineDelay { get; internal set; }

        public string MappingFilePathPrefix { get; internal set; }

        public IEnumerable<MappingItem> Mapping { get; internal set; }

        public class MappingItem
        {
            public Location Src { get; internal set; }
            public Location Dst { get; internal set; }

            public override string ToString()
            {
                return $"src=\"{Src.Word}\" to dst=\"{Dst.Word}\"";
            }

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