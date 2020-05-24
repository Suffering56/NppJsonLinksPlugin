using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace NppPluginForHC.Logic.Parser.Json
{
    public class DefaultJsonParser : IDocumentParser
    {
        public void ParseValidDocument(string filePath, ICollection<Word> expectedWords, ValueConsumer valueConsumer)
        {
            string expectedWord = null;
            string currentPropertyName = null;

            Stack<string> propertyStack = new Stack<string>();
            propertyStack.Push(AppConstants.RootPropertyName);

            using JsonTextReader reader = new JsonTextReader(new StreamReader(filePath));
            while (reader.Read())
            {
                foreach (var dstWord in expectedWords)
                {
                    var tokenType = reader.TokenType;
                    object value = reader.Value;
                    int lineNumber = reader.LineNumber;

                    if (!dstWord.IsComplex())
                    {
                        ParseSimpleWord(tokenType, value, dstWord, ref expectedWord, val => valueConsumer.Invoke(dstWord, lineNumber, val));
                    }
                    else
                    {
                        ParseComplexWord(tokenType, value, dstWord, ref currentPropertyName, propertyStack, val => valueConsumer.Invoke(dstWord, lineNumber, val));
                    }
                }
            }
        }

        public void ParseInvalidDocument(string filePath, ICollection<Word> expectedWords, ValueConsumer valueConsumer)
        {
            int lineNumber = 0;
            string lineText;
            using StreamReader sr = new StreamReader(filePath);
            while ((lineText = sr.ReadLine()) != null)
            {
                foreach (var dstWord in expectedWords)
                {
                    var dstWordString = dstWord.WordString;

                    if (!lineText.Contains($"\"{dstWordString}\"")) continue;

                    string value = ExtractTokenValueByLine(lineText, dstWordString);
                    if (value != null)
                    {
                        valueConsumer.Invoke(dstWord, lineNumber, value);
                    }
                }

                lineNumber++;
            }
        }

        private static void ParseComplexWord(JsonToken tokenType, object? value, Word dstWord, ref string propertyName, Stack<string> propertyStack, Action<string> valueConsumer)
        {
            switch (tokenType)
            {
                case JsonToken.StartObject:
                case JsonToken.StartArray:

                    if (propertyName != null)
                    {
                        propertyStack.Push(propertyName);
                        propertyName = null;
                    }
                    else
                    {
                        propertyStack.Push(null);
                    }

                    return;

                case JsonToken.EndObject:
                case JsonToken.EndArray:
                    if (propertyStack.Count > 0)
                    {
                        propertyStack.Pop();
                    }

                    return;
            }

            if (value == null) return;

            if (tokenType == JsonToken.PropertyName)
            {
                propertyName = value.ToString();
                return;
            }

            // value не принадлежит никакой property - выходим, ибо я не знаю как обработать это, да и в общем-то воспроизвести тоже
            if (propertyName == null) return;

            string expectedPropertyName = propertyName;
            propertyName = null;

            // это просто property, которое не участвует в маппинге
            if (dstWord.WordString != expectedPropertyName) return;

            string valueString = value.ToString();
            switch (tokenType)
            {
                case JsonToken.Boolean:
                    valueString = valueString.ToLower();
                    break;

                case JsonToken.Float:
                    valueString = valueString.Replace(',', '.');
                    break;

                case JsonToken.Integer:
                case JsonToken.String:
                    break;
                default:
                    // пришло что-то странное, пропускаем эту пропертю
                    return;
            }


            var parent = dstWord.Parent;
            foreach (var stackItem in propertyStack)
            {
                if (stackItem == null) continue;
                if (parent.WordString != stackItem) return;

                parent = parent.Parent;
                if (parent != null) continue;

                // все совпало, это наш токен. сохраняем значение
                valueConsumer.Invoke(valueString);
                return;
            }

            // стек закончился, а нам dstWord нет. значит это не тот токен
        }

        private static void ParseSimpleWord(JsonToken tokenType, object? value, Word dstWord, ref string expectedWord, Action<string> valueConsumer)
        {
            if (value == null) return;

            //ожидаем property
            if (tokenType == JsonToken.PropertyName) // TODO: or StartToken/EndToken/etc..
            {
                expectedWord = null;

                if (dstWord.WordString == value.ToString())
                {
                    expectedWord = dstWord.WordString;
                }

                return;
            }

            if (expectedWord != dstWord.WordString) return;

            //ожидаем value
            string valueString = value.ToString();
            switch (tokenType)
            {
                case JsonToken.Boolean:
                    valueString = valueString.ToLower();
                    break;

                case JsonToken.Float:
                    valueString = valueString.Replace(',', '.');
                    break;

                case JsonToken.Integer:
                case JsonToken.String:
                    break;
                default:
                    return;
            }

            valueConsumer.Invoke(valueString);
            expectedWord = null;
        }

        private const string TokenValuePattern = "^.*\"[PROPERTY_NAME]\"\\s*:\\s*\"?([\\w|\\.]+)\"?\\s*";

        private static string ExtractTokenValueByLine(string lineText, string propertyName)
        {
            string pattern = new StringBuilder(TokenValuePattern).Replace("[PROPERTY_NAME]", propertyName).ToString();

            var match = new Regex(pattern).Match(lineText);
            if (!match.Success) return null;

            var matchGroup = match.Groups[1];
            return matchGroup.Success
                ? matchGroup.Value
                : null;
        }
    }
}