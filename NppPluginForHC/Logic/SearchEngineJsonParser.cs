#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NppPluginForHC.Core;
using static NppPluginForHC.Logic.DefinitionSearchEngine;

namespace NppPluginForHC.Logic
{
    public class SearchEngineJsonParser
    {
        private readonly IDictionary<Word, ValuesLocationContainer> _valuesContainerByWordMap;

        private delegate void ValueConsumer(string value);

        public SearchEngineJsonParser(IDictionary<Word, ValuesLocationContainer> valuesContainerByWordMap)
        {
            _valuesContainerByWordMap = valuesContainerByWordMap;
        }

        public void TryParseValidJson(string filePath)
        {
            string expectedWord = null;
            string currentPropertyName = null;

            Stack<string> propertyStack = new Stack<string>();
            propertyStack.Push(Settings.RootTokenPropertyName);

            // using JsonTextReader reader = new JsonTextReader(new StreamReader("F:/gd_data/convertPrices.json"));
            using JsonTextReader reader = new JsonTextReader(new StreamReader(filePath));
            while (reader.Read())
            {
                foreach (var entry in _valuesContainerByWordMap)
                {
                    var dstWord = entry.Key;
                    var valuesContainer = entry.Value;

                    var tokenType = reader.TokenType;
                    object value = reader.Value;

                    if (!dstWord.IsComplex())
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        ParseSimpleWord(tokenType, value, dstWord, ref expectedWord, val => valuesContainer.PutOrReplace(val, reader.LineNumber));
                    }
                    else
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        ParseComplexWord(tokenType, value, dstWord, ref currentPropertyName, propertyStack, val => valuesContainer.PutOrReplace(val, reader.LineNumber));
                    }
                }
            }
        }

        private static void ParseComplexWord(JsonToken tokenType, object? value, Word dstWord, ref string propertyName, Stack<string> propertyStack, ValueConsumer valueConsumer)
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
                // Print(propertyName, valueString, propertyStack);
                return;
            }

            // стек закончился, а нам dstWord нет. значит это не тот токен
        }

        private static void Print(string propertyName, string propertyValue, Stack<string> propertyStack)
        {
            string propertyPath = propertyName + Word.WordSeparator + propertyStack.Aggregate((current, next) =>
            {
                if (next == null)
                {
                    return current;
                }

                if (current == null)
                {
                    return next;
                }

                return current + Word.WordSeparator + next;
            });

            // Console.WriteLine($"{propertyPath}\t\t\t={propertyValue}");
            Logger.Info($"{propertyPath}\t\t\t={propertyValue}");
        }

        private static void ParseSimpleWord(JsonToken tokenType, object? value, Word dstWord, ref string expectedWord, ValueConsumer valueConsumer)
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

        public void ParseInvalidJson(string filePath)
        {
            int lineNumber = 0;
            string lineText;
            using StreamReader sr = new StreamReader(filePath);
            while ((lineText = sr.ReadLine()) != null)
            {
                foreach (var entry in _valuesContainerByWordMap)
                {
                    var dstWordString = entry.Key.WordString;
                    var valuesContainer = entry.Value;

                    if (!lineText.Contains($"\"{dstWordString}\"")) continue;

                    string value = Utils.ExtractTokenValueByLine(lineText, dstWordString);
                    if (value != null)
                    {
                        valuesContainer.PutOrReplace(value, lineNumber);
                    }
                }

                lineNumber++;
            }
        }
    }
}