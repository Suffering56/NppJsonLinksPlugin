using System;
using System.Text;
using System.Text.RegularExpressions;
using NppPluginForHC.Configuration;

namespace NppPluginForHC.Logic.Parser.Json
{
    public static class JsonStringUtils
    {
        //TODO не учитываются строки с нецифрами и небуквами
        private const string TokenValuePattern = "^.*\"[PROPERTY_NAME]\"\\s*:\\s*\"?([\\w|\\.]+)\"?\\s*";
        public delegate string LineTextProvider(int lineIndex);

        private static readonly TokenResult RootTokenResult = new TokenResult(Settings.RootTokenPropertyName, 0);
        private const char ExpectNamedObjectChar = ':';
        private const char PropertyBorderChar = '"';
        private const string EmptyString = "";

        public static string ExtractTokenValueByLine(string lineText, string propertyName)
        {
            string pattern = new StringBuilder(TokenValuePattern).Replace("[PROPERTY_NAME]", propertyName).ToString();

            var match = new Regex(pattern).Match(lineText);
            if (!match.Success) return null;

            var matchGroup = match.Groups[1];
            return matchGroup.Success
                ? matchGroup.Value
                : null;
        }

        public static TokenResult GetParentToken(string initialPropertyName, int initialLineIndex, LineTextProvider lineTextProvider)
        {
            var tokenCounter = 0;
            char? expectedChar = null;
            string propertyName = null;

            var lineText = lineTextProvider.Invoke(initialLineIndex);
            var lastLineCharIndex = lineText.IndexOf($"\"{initialPropertyName}\"", StringComparison.Ordinal);
            if (lastLineCharIndex == -1)
            {
                throw new Exception($"currentWordStr={initialPropertyName} not found in line={initialLineIndex}");
            }

            for (int lineIdx = initialLineIndex; lineIdx >= 0; lineIdx--)
            {
                lineText = lineTextProvider.Invoke(lineIdx);
                if (lineIdx < initialLineIndex)
                {
                    lastLineCharIndex = lineText.Length;
                }

                for (int charIdx = lastLineCharIndex - 1; charIdx >= 0; charIdx--)
                {
                    char ch = lineText[charIdx];

                    if (IsEndTokenChar(ch))
                    {
                        if (expectedChar == ExpectNamedObjectChar)
                        {
                            // unnamed parent found; try search next parent
                            tokenCounter = 0;
                            expectedChar = null;
                            continue;
                        }

                        tokenCounter--;
                        continue;
                    }

                    if (ch == expectedChar)
                    {
                        switch (expectedChar)
                        {
                            case ExpectNamedObjectChar:
                                expectedChar = PropertyBorderChar;
                                break;
                            case PropertyBorderChar when propertyName == null:
                                // property start
                                propertyName = EmptyString;
                                break;
                            case PropertyBorderChar:
                                // property end
                                return new TokenResult(propertyName, lineIdx);
                        }

                        continue;
                    }

                    if (propertyName != null)
                    {
                        propertyName = ch + propertyName;
                        continue;
                    }


                    if (IsStartTokenChar(ch))
                    {
                        if (tokenCounter == 0)
                        {
                            expectedChar = ExpectNamedObjectChar;
                        }

                        tokenCounter++;
                    }
                }
            }

            return RootTokenResult;
        }

        private static bool IsStartTokenChar(char ch)
        {
            return ch == '{' || ch == '[';
        }

        private static bool IsEndTokenChar(char ch)
        {
            return ch == '}' || ch == ']';
        }

        public class TokenResult
        {
            public readonly string PropertyName;
            public readonly int PropertyLine;

            public TokenResult(string propertyName, int propertyLine)
            {
                PropertyName = propertyName;
                PropertyLine = propertyLine;
            }
        }
    }
}