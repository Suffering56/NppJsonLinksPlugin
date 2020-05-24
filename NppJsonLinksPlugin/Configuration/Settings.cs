using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
                .SelectMany(rawMappingItem =>
                {
                    return rawMappingItem.Src.Select(srcLocation => new Settings.MappingItem
                    {
                        Src = new Settings.MappingItem.SrcLocation
                        (
                            Word.Parse(srcLocation.Word),
                            srcLocation.FileName,
                            rawSettings.MappingDefaultFilePath,
                            srcLocation.OverrideFilePath
                        ),
                        Dst = new Settings.MappingItem.DstLocation
                        (
                            Word.Parse(rawMappingItem.Dst.Word),
                            rawMappingItem.Dst.FileName,
                            rawSettings.MappingDefaultFilePath,
                            rawMappingItem.Dst.OverrideFilePath
                        )
                    });
                })
                .ToList();

            return new Settings
            {
                LoggerMode = rawSettings.LoggerMode,
                LogPathPrefix = rawSettings.LogPathPrefix,
                CacheEnabled = rawSettings.CacheEnabled,
                SoundEnabled = rawSettings.SoundEnabled,
                JumpToLineDelay = rawSettings.JumpToLineDelay,
                MappingFilePathPrefix = rawSettings.MappingDefaultFilePath,
                Mapping = mappingItems
            };
        }

        private static void Validate(Settings settings)
        {
            Check(!string.IsNullOrWhiteSpace(settings.MappingFilePathPrefix), "settings.MappingFilePathPrefix cannot be null or empty");

            int index = 0;
            foreach (var mappingItem in settings.Mapping)
            {
                Check(!string.IsNullOrWhiteSpace(mappingItem.Src.FilePath), $"FilePath is empty! mappingItem[{index}]={mappingItem}");
                Check(!mappingItem.Dst.FullPath.Contains("*"), $"FullFile path could not contains asterisk (*)! mappingItem[{index}]={mappingItem}");
                index++;
            }

            //TODO: fileName not contains regexp spec-symbols, but src.FilePath can contains '*'
            //TODO: Mapping.Src must be unique
        }

        private static void Check(bool expression, string errorMsg, params object[] errorArgs)
        {
            if (!expression)
                throw new ValidationException(string.Format(errorMsg, errorArgs));
        }

        private class ValidationException : Exception
        {
            public ValidationException(string message) : base(message)
            {
            }
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
            public SrcLocation Src { get; internal set; }
            public DstLocation Dst { get; internal set; }

            public override string ToString()
            {
                return $"[Src={Src}, Dst={Dst}]";
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


            public class SrcLocation
            {
                public SrcLocation(Word word, string fileName, string defaultFilePath, string overrideFilePath)
                {
                    _initialFileName = fileName;
                    Word = word;
                    FilePath = StringSupport.NormalizePath(!string.IsNullOrWhiteSpace(overrideFilePath)
                        ? overrideFilePath
                        : defaultFilePath
                    );

                    if (FilePath.EndsWith("\\") || FilePath.EndsWith("/"))
                    {
                        FilePath = FilePath.Substring(0, FilePath.Length - 1);
                    }

                    if (!fileName.Contains('*'))
                    {
                        _fileName = fileName;
                        _fileNamePattern = null;
                    }
                    else
                    {
                        _fileName = null;
                        _fileNamePattern = ToPattern(fileName);
                    }
                }

                public readonly Word Word;
                private readonly string _initialFileName;
                internal readonly string FilePath;

                private readonly string _fileName;
                private readonly string _fileNamePattern;

                private const string AnyFile = "*";
                private const string AsteriskPattern = "[^:/\\\\*?\"<>]+";

                private static string ToPattern(string fileName)
                {
                    var rawPattern = new StringBuilder(fileName)
                        .Replace("*.*", "*")
                        .ToString();

                    if (rawPattern.EndsWith("*"))
                    {
                        rawPattern += "$";
                    }

                    return rawPattern.Replace("*", AsteriskPattern);
                }

                public bool MatchesWithPath(string absoluteNormalizedFilePath)
                {
                    var fileName = Path.GetFileName(absoluteNormalizedFilePath);
                    if (fileName == null)
                    {
                        // ReSharper disable once ExpressionIsAlwaysNull
                        Logger.Warn($"could not extract file name from path: {absoluteNormalizedFilePath}");
                        return false;
                    }

                    if (FilePath != AnyFile)
                    {
                        var directoryName = Path.GetDirectoryName(absoluteNormalizedFilePath);

                        if (directoryName != FilePath)
                        {
                            return false;
                        }
                    }

                    if (_fileName != null)
                    {
                        return fileName == _fileName;
                    }

                    return Regex.IsMatch(fileName, _fileNamePattern);
                }

                public override string ToString()
                {
                    return $"word={Word},    fullPath=\"{FilePath}\\{_initialFileName}\"";
                }

                private bool Equals(SrcLocation other)
                {
                    return Equals(Word, other.Word) && _initialFileName == other._initialFileName && FilePath == other.FilePath;
                }

                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(null, obj)) return false;
                    if (ReferenceEquals(this, obj)) return true;
                    if (obj.GetType() != this.GetType()) return false;
                    return Equals((SrcLocation) obj);
                }

                public override int GetHashCode()
                {
                    unchecked
                    {
                        var hashCode = (Word != null ? Word.GetHashCode() : 0);
                        hashCode = (hashCode * 397) ^ (_initialFileName != null ? _initialFileName.GetHashCode() : 0);
                        hashCode = (hashCode * 397) ^ (FilePath != null ? FilePath.GetHashCode() : 0);
                        return hashCode;
                    }
                }
            }

            public class DstLocation
            {
                public readonly Word Word;
                public readonly string FullPath;

                public DstLocation(Word word, string fileName, string defaultFilePath, string overrideFilePath)
                {
                    string pathPrefix = !string.IsNullOrWhiteSpace(overrideFilePath)
                        ? overrideFilePath
                        : defaultFilePath;

                    Word = word;
                    FullPath = StringSupport.NormalizePath($"{pathPrefix}\\{fileName}");
                }

                public override string ToString()
                {
                    return $"[word={Word}, fullPath={FullPath}]";
                }

                private bool Equals(DstLocation other)
                {
                    return Equals(Word, other.Word) && FullPath == other.FullPath;
                }

                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(null, obj)) return false;
                    if (ReferenceEquals(this, obj)) return true;
                    if (obj.GetType() != this.GetType()) return false;
                    return Equals((DstLocation) obj);
                }

                public override int GetHashCode()
                {
                    unchecked
                    {
                        return ((Word != null ? Word.GetHashCode() : 0) * 397) ^ (FullPath != null ? FullPath.GetHashCode() : 0);
                    }
                }
            }
        }
    }
}