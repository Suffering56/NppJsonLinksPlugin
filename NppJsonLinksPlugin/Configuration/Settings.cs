using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using NppJsonLinksPlugin.Core;
using NppJsonLinksPlugin.Logic;
using static NppJsonLinksPlugin.AppConstants;

namespace NppJsonLinksPlugin.Configuration
{
    public static class SettingsParser
    {
        public static Settings LoadMapping(IniConfig iniConfig)
        {
            if (!File.Exists(MAPPING_LOCAL_PATH))
            {
                DownloadRemoteMapping(ConvertUtils.ToUri(iniConfig.MappingRemoteUrl));
            }
            else
            {
                Logger.Info($"{MAPPING_LOCAL_PATH} exists! Mapping will be loaded locally");
            }

            RawMapping rawMapping = LoadRawMapping();
            Settings settings = MergeToSettings(rawMapping, iniConfig);
            Validate(settings);
            
            Logger.Info($"Mapping successfully reloaded from: {MAPPING_LOCAL_PATH}");
            return settings;
        }

        private static RawMapping LoadRawMapping()
        {
            try
            {
                string mappingString = File.ReadAllText(MAPPING_LOCAL_PATH);
                var rawMapping = JsonConvert.DeserializeObject<RawMapping>(mappingString);
                return rawMapping;
            }
            catch (Exception e)
            {
                Logger.Error($"cannot parse mapping by uri: {MAPPING_LOCAL_PATH}", e, true);
                throw;
            }
        }

        public static void DownloadRemoteMapping(Uri remoteMappingUrl)
        {
            Logger.Info($"loading mapping from: {remoteMappingUrl}");
            
            WebRequest request = WebRequest.Create(remoteMappingUrl);
            request.Timeout = 30 * 60 * 1000;
            request.UseDefaultCredentials = true;
            request.Proxy.Credentials = request.Credentials;
            WebResponse response = request.GetResponse();

            Stream inputStream = response.GetResponseStream() ?? throw new Exception($"couldn't read settings by uri: {remoteMappingUrl}. response stream is null");

            if (!Directory.Exists(MAPPING_LOCAL_DIR))
            {
                Directory.CreateDirectory(MAPPING_LOCAL_DIR);
            }

            using Stream outputStream = File.Create(MAPPING_LOCAL_PATH);
            inputStream.CopyTo(outputStream);
            
            Logger.Info($"loading mapping result: SUCCESS");
        }

        private static Settings MergeToSettings(RawMapping rawMapping, IniConfig iniConfig)
        {
            var mappingDefaultFilePath = iniConfig.WorkingDirectory;

            var mappingItems = rawMapping.Mapping
                .SelectMany(rawMappingItem =>
                {
                    return rawMappingItem.Src.Select(srcLocation => new Settings.MappingItem
                    {
                        Src = new Settings.MappingItem.SrcLocation
                        (
                            Word.ParseSrc(srcLocation.Word, srcLocation.WordRegexpEnabled),
                            srcLocation.FileName,
                            srcLocation.Order,
                            mappingDefaultFilePath,
                            srcLocation.OverrideFilePath,
                            srcLocation.FileNameRegexpEnabled,
                            srcLocation.IgnoredFileNames
                        ),
                        Dst = new Settings.MappingItem.DstLocation
                        (
                            Word.ParseDst(rawMappingItem.Dst.Word),
                            rawMappingItem.Dst.FileName,
                            mappingDefaultFilePath,
                            rawMappingItem.Dst.OverrideFilePath
                        )
                    });
                })
                .ToList();

            return new Settings(iniConfig, mappingItems);
        }

        private static void Validate(Settings settings)
        {
            int index = 0;
            foreach (var mappingItem in settings.Mapping)
            {
                Check(!string.IsNullOrWhiteSpace(mappingItem.Src.FilePath), $"FilePath is empty! mappingItem[{index}]={mappingItem}");
                Check(!mappingItem.Dst.FullPath.Contains("*"), $"FullFile path could not contains asterisk (*)! mappingItem[{index}]={mappingItem}");
                index++;
            }

            //TODO: dst fileName not contains regexp spec-symbols, but src.FilePath can contains '*'
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
        public readonly IniConfig Config;
        public readonly IEnumerable<MappingItem> Mapping;

        public Settings(IniConfig config, IEnumerable<MappingItem> mapping)
        {
            Config = config;
            Mapping = mapping;
        }

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
                public readonly Word Word;
                private readonly string _initialFileName;
                internal readonly string FilePath;
                private readonly ISet<string> _ignoredFileNames;
                public readonly int Order;

                private readonly string _fileName;
                private readonly string _fileNamePattern;

                private const string ANY_FILE = "*";
                private const string ASTERISK_PATTERN = "[^:/\\\\*?\"<>]*";

                public SrcLocation(Word word, string fileName, int order, string defaultFilePath, string overrideFilePath, bool fileNameRegexpEnabled, List<string> ignoredFileNames)
                {
                    Order = order;
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

                    _ignoredFileNames = new HashSet<string>(ignoredFileNames
                        .Select(ignoredFileName => Path.Combine(FilePath, ignoredFileName))
                        .Select(StringUtils.NormalizePath)
                    );

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

                private static string ToRegexp(string fileName)
                {
                    var rawPattern = new StringBuilder(fileName)
                        .Replace("*.*", "*")
                        .ToString();

                    return "^" + rawPattern.Replace("*", ASTERISK_PATTERN) + "$";
                }

                public bool MatchesWithPath(string absoluteNormalizedFilePath)
                {
                    var fileName = Path.GetFileName(absoluteNormalizedFilePath);
                    if (fileName == null)
                    {
                        Logger.Warn($"could not extract file name from path: {absoluteNormalizedFilePath}");
                        return false;
                    }

                    if (_ignoredFileNames.Contains(absoluteNormalizedFilePath))
                    {
                        return false;
                    }

                    if (FilePath != ANY_FILE)
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

                    return Regex.IsMatch(fileName, _fileNamePattern, RegexOptions.IgnoreCase);
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