using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Newtonsoft.Json;
using NppJsonLinksPlugin.Core;
using NppJsonLinksPlugin.Logic;

namespace NppJsonLinksPlugin.Configuration
{
    public static class SettingsParser
    {
        public static Settings Load(IniConfig iniConfig)
        {
            var rawSettings = Parse(iniConfig.SettingsJsonUri);
            Settings settings = ConvertRawSettings(rawSettings, iniConfig);

            Validate(settings);

            return settings;
        }

        public static RawSettings Parse(string settingsUri)
        {
            var uri = ToUri(settingsUri);
            string settingsString = uri.IsFile
                ? File.ReadAllText(uri.AbsolutePath)
                : ReadRemoteSettings(uri);

            var rawSettings = JsonConvert.DeserializeObject<RawSettings>(settingsString);
            return rawSettings;
        }

        private static Uri ToUri(string uri)
        {
            try
            {
                return new Uri(uri);
            }
            catch (Exception)
            {
                Logger.Error($"URI is not valid: \"{uri}\"", null, true);
                throw;
            }
        }

        private static string ReadRemoteSettings(Uri uri)
        {
            WebRequest request = WebRequest.Create(uri);
            request.Timeout = 30 * 60 * 1000;
            request.UseDefaultCredentials = true;
            request.Proxy.Credentials = request.Credentials;
            WebResponse response = request.GetResponse();

            using StreamReader reader = new StreamReader(
                response.GetResponseStream()
                ?? throw new Exception($"couldn't read settings by uri: {uri}. response stream is null")
            );
            return reader.ReadToEnd();
        }

        private static Settings ConvertRawSettings(RawSettings rawSettings, IniConfig iniConfig)
        {
            var mappingItems = rawSettings.Mapping
                .SelectMany(rawMappingItem =>
                {
                    return rawMappingItem.Src.Select(srcLocation => new Settings.MappingItem
                    {
                        Src = new Settings.MappingItem.SrcLocation
                        (
                            Word.ParseSrc(srcLocation.Word, srcLocation.WordRegexpEnabled),
                            srcLocation.FileName,
                            rawSettings.MappingDefaultFilePath,
                            srcLocation.OverrideFilePath,
                            srcLocation.FileNameRegexpEnabled
                        ),
                        Dst = new Settings.MappingItem.DstLocation
                        (
                            Word.ParseDst(rawMappingItem.Dst.Word),
                            rawMappingItem.Dst.FileName,
                            rawSettings.MappingDefaultFilePath,
                            rawMappingItem.Dst.OverrideFilePath
                        )
                    });
                })
                .ToList();

            return MergeWithConfig(rawSettings, iniConfig, mappingItems);
        }

        private static Settings MergeWithConfig(RawSettings rawSettings, IniConfig iniConfig, List<Settings.MappingItem> mappingItems)
        {
            return new Settings
            {
                HighlightingEnabled = iniConfig.HighlightingEnabled ?? rawSettings.HighlightingEnabled, // override by ini
                ProcessingHighlightedLinesLimit = iniConfig.ProcessingHighlightedLinesLimit ?? rawSettings.ProcessingHighlightedLinesLimit, // override by ini 
                SoundEnabled = iniConfig.SoundEnabled ?? rawSettings.SoundEnabled, // override by ini
                JumpToLineDelay = iniConfig.JumpToLineDelay ?? rawSettings.JumpToLineDelay, // override by ini
                MappingDefaultFilePath = iniConfig.MappingDefaultFilePath ?? rawSettings.MappingDefaultFilePath, // override by ini
                Mapping = mappingItems
            };
        }

        private static void Validate(Settings settings)
        {
            Check(!string.IsNullOrWhiteSpace(settings.MappingDefaultFilePath), "settings.MappingDefaultFilePath cannot be null or empty");

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
                // do nothing
            }
        }
    }

    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public class Settings
    {
        public bool HighlightingEnabled { get; internal set; }

        public int ProcessingHighlightedLinesLimit { get; internal set; }

        public bool SoundEnabled { get; internal set; }

        public int JumpToLineDelay { get; internal set; }

        public string MappingDefaultFilePath { get; internal set; }

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
                if (obj.GetType() != GetType()) return false;
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
                public SrcLocation(Word word, string fileName, string defaultFilePath, string overrideFilePath, bool fileNameRegexpEnabled)
                {
                    _initialFileName = fileName;
                    Word = word;
                    FilePath = StringUtils.NormalizePath(!string.IsNullOrWhiteSpace(overrideFilePath)
                        ? overrideFilePath
                        : defaultFilePath
                    );

                    if (FilePath.EndsWith("\\") || FilePath.EndsWith("/"))
                    {
                        FilePath = FilePath.Substring(0, FilePath.Length - 1);
                    }

                    if (fileNameRegexpEnabled)
                    {
                        _fileName = null;
                        _fileNamePattern = _fileName;
                    }
                    else if (fileName.Contains('*'))
                    {
                        _fileName = null;
                        _fileNamePattern = ToRegexp(fileName);
                    }
                    else
                    {
                        _fileName = fileName;
                        _fileNamePattern = null;
                    }
                }

                public readonly Word Word;
                private readonly string _initialFileName;
                internal readonly string FilePath;

                private readonly string _fileName;
                private readonly string _fileNamePattern;

                private const string AnyFile = "*";
                private const string AsteriskPattern = "[^:/\\\\*?\"<>]+";

                private static string ToRegexp(string fileName)
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
                    FullPath = StringUtils.NormalizePath($"{pathPrefix}\\{fileName}");
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