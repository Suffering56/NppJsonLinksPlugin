using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace NppJsonLinksPlugin.Logic.Parser.Json
{
    public class DefaultJsonParser : IDocumentParser
    {
        public void ParseValidDocument(string filePath, ICollection<Word> expectedWords, ValueConsumer valueConsumer)
        {
            string expectedWord = null;
            string propertyName = null;

            Stack<string> propertyStack = new Stack<string>();
            propertyStack.Push(AppConstants.ROOT_PROPERTY_NAME);

            var hasComplexWords = expectedWords.Any(word => word.IsComplex());

            using JsonTextReader reader = new JsonTextReader(new StreamReader(filePath));
            while (reader.Read())
            {
                var tokenType = reader.TokenType;
                object value = reader.Value;
                int lineNumber = reader.LineNumber - 1;

                string foundPropertyName = null;
                if (hasComplexWords)
                {
                    foundPropertyName = ExtractPropertyName(tokenType, propertyStack, value, ref propertyName);
                }

                foreach (var dstWord in expectedWords)
                {
                    if (!dstWord.IsComplex())
                    {
                        ParseSimpleWord(tokenType, value, dstWord, ref expectedWord, val => valueConsumer.Invoke(dstWord, lineNumber, val));
                    }
                    else
                    {
                        if (foundPropertyName == null) continue;
                        ParseComplexWord(tokenType, value, dstWord, foundPropertyName, propertyStack, val => valueConsumer.Invoke(dstWord, lineNumber, val));
                    }
                }
            }
        }

        private static void ParseComplexWord(JsonToken tokenType, object value, Word dstWord, string foundPropertyName, Stack<string> propertyStack, Action<string> valueConsumer)
        {
            // это просто property, которое не участвует в маппинге
            if (dstWord.GetWordString() != foundPropertyName) return;

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
                if (parent.GetWordString() != stackItem) return;

                parent = parent.Parent;
                if (parent != null) continue;

                // все совпало, это наш токен. сохраняем значение
                valueConsumer.Invoke(valueString);
                return;
            }

            // стек закончился, а нам dstWord нет. значит это не тот токен
        }

        private static string ExtractPropertyName(JsonToken tokenType, Stack<string> propertyStack, object value, ref string propertyName)
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

                    return null;

                case JsonToken.EndObject:
                case JsonToken.EndArray:
                    if (propertyStack.Count > 0)
                    {
                        propertyStack.Pop();
                    }

                    return null;
            }

            if (value == null) return null;

            if (tokenType == JsonToken.PropertyName)
            {
                propertyName = value.ToString();
                return null;
            }

            // value не принадлежит никакой property - выходим, ибо я не знаю как обработать это, да и в общем-то воспроизвести тоже
            if (propertyName == null) return null;

            string foundPropertyName = propertyName;
            propertyName = null;
            return foundPropertyName;
        }

        private static void ParseSimpleWord(JsonToken tokenType, object? value, Word dstWord, ref string expectedWord, Action<string> valueConsumer)
        {
            if (value == null) return;

            //ожидаем property
            if (tokenType == JsonToken.PropertyName) // TODO: or StartToken/EndToken/etc..
            {
                expectedWord = null;

                if (dstWord.GetWordString() == value.ToString())
                {
                    expectedWord = dstWord.GetWordString();
                }

                return;
            }

            if (expectedWord != dstWord.GetWordString()) return;

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
    }
}