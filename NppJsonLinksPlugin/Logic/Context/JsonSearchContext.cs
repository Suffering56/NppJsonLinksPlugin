using System.Diagnostics.CodeAnalysis;
using System.Text;
using NppJsonLinksPlugin.Core;
using NppJsonLinksPlugin.PluginInfrastructure.Gateway;

namespace NppJsonLinksPlugin.Logic.Context
{
    public class JsonSearchContext : ISearchContext
    {
        private readonly Property _selectedProperty;

        private readonly string _selectedWord;
        private readonly IScintillaGateway _gateway;
        private readonly int _totalLinesCount;

        public JsonSearchContext(string selectedWord, IScintillaGateway gateway)
        {
            _gateway = gateway;
            _selectedWord = selectedWord;
            _totalLinesCount = _gateway.GetLineCount();

            var initialLineIndex = _gateway.GetCurrentLine();
            var indexOfSelectedWord = _gateway.GetCurrentPos().Value - _gateway.LineToPosition(initialLineIndex) - _selectedWord.Length;
            _selectedProperty = TryInitSelectedProperty(initialLineIndex, indexOfSelectedWord);
        }

        public bool MatchesWith(Word expectedWord)
        {
            var propertyName = _selectedProperty.Name;

            if (!expectedWord.MatchesWith(propertyName)) return false;
            if (!expectedWord.IsComplex()) return true;

            var lineIndex = _selectedProperty.NameLineIndex;
            var lineOffset = _selectedProperty.NameLineOffset;

            Word parent = expectedWord;
            while ((parent = parent.Parent) != null)
            {
                var propertyResult = ExtractParentPropertyName(lineIndex, lineOffset - 1);
                if (!parent.MatchesWith(propertyResult.PropertyName))
                {
                    Logger.Fail($"expectedWord: [{expectedWord}] does not match with selectedProperty: [{_selectedProperty}], because {propertyResult.PropertyName} != {parent.GetWordString()}");
                    return false;
                }

                lineIndex = propertyResult.StopLineIndex;
                lineOffset = propertyResult.StopLineOffset;
            }

            return true;
        }

        public bool IsValid()
        {
            return _selectedProperty != null;
        }

        public string GetSelectedWord()
        {
            return _selectedWord;
        }

        public Property GetSelectedProperty()
        {
            return _selectedProperty;
        }

        private string GetLineText(int lineIndex)
        {
            if (lineIndex < 0 || lineIndex >= _totalLinesCount)
            {
                Logger.Error($"incorrect lineIndex={lineIndex}, must be in closeOpenRange: [0..{_totalLinesCount})");
                return null;
            }

            return _gateway.GetLineText(lineIndex);
        }

        private Property TryInitSelectedProperty(int initialLineIndex, int indexOfSelectedWord)
        {
            string lineText = GetLineText(initialLineIndex);
            var direction = ChooseDirection(_selectedWord, indexOfSelectedWord, lineText);

            if (direction == null)
            {
                Logger.Fail($"direction is null for selectedWord: {_selectedWord}");
                return null;
            }

            return FindPropertyByDirection(initialLineIndex, indexOfSelectedWord, direction);
        }

        private Property FindPropertyByDirection(int initialLineIndex, int indexOfSelectedWord, Direction? direction)
        {
            switch (direction)
            {
                case Direction.LEFT:
                    int lineOffset = indexOfSelectedWord - 1;
                    int lineIndex = initialLineIndex;
                    if (lineOffset == -1) lineIndex--;

                    var propertyNameLocation = GoLeftForPropertyName(lineIndex, lineOffset);
                    if (propertyNameLocation != null) return new Property(propertyNameLocation, _selectedWord);
                    break;

                case Direction.RIGHT:
                    // selectedWord окружено кавычками. наиболее вероятно что selectedWord это propertyName, идем на право за propertyValue
                    var rightResult = GoRightForPropertyValue(initialLineIndex, indexOfSelectedWord + _selectedWord.Length + 1); // +1 потому что скипаем кавычку "
                    // встречено что-то невалидное или неожиданное (и неподдержанное мною)
                    if (rightResult == null) return null;

                    if (rightResult.IsNeedGoLeft())
                    {
                        // мы поняли, что selectedWord - это stringPropertyValue, идем налево за propertyName
                        return FindPropertyByDirection(initialLineIndex, indexOfSelectedWord, Direction.LEFT);
                    }
                    else
                    {
                        // мы успешно нашли propertyValue (после двоеточия как положено)
                        return new Property(_selectedWord, initialLineIndex, indexOfSelectedWord, rightResult.PropertyValue);
                    }
            }

            return null;
        }

        private RightResult GoRightForPropertyValue(int lineIndex, int lineOffset)
        {
            if (!IsValidLineIndex(lineIndex)) return null;
            string lineText = GetLineText(lineIndex);

            int lineLength = lineText.Length;
            for (int i = lineOffset;
                i < lineLength;
                i++)
            {
                var ch = lineText[i];

                if (ch.IsWhiteSpace()) continue;

                if (ch == ':')
                {
                    var propertyValue = ExtractPropertyValue(lineIndex, i + 1);

                    return propertyValue != null
                        ? new RightResult(propertyValue)
                        : null;
                }

                return ch.IsOneOf(',', ']', '}')
                    ? RightResult.GoLeft()
                    : null;
            }

            // ReSharper disable once TailRecursiveCall
            return GoRightForPropertyValue(lineIndex + 1, 0);
        }

        private PropertyNameLocation GoLeftForPropertyName(int lineIndex, int lineOffset)
        {
            if (!IsValidLineIndex(lineIndex)) return null;

            string lineText = GetLineText(lineIndex);

            if (lineOffset == -1) lineOffset = lineText.Length - 1;

            for (int i = lineOffset; i >= 0; i--)
            {
                char ch = lineText[i];

                if (ch.IsWhiteSpace()) continue;

                switch (ch)
                {
                    case ':':
                        return ExtractCurrentPropertyName(lineIndex, i - 1);
                    case ',':
                    case '[':
                        return ExtractParentPropertyName(lineIndex, i);
                }
            }

            return GoLeftForPropertyName(lineIndex - 1, -1);
        }

        private PropertyNameLocation ExtractCurrentPropertyName(int lineIndex, int lineOffset)
        {
            if (!IsValidLineIndex(lineIndex)) return null;

            string lineText = GetLineText(lineIndex);
            if (lineOffset == -1) lineOffset = lineText.Length - 1;

            for (int i = lineOffset; i >= 0; i--)
            {
                var ch = lineText[i];

                if (ch.IsWhiteSpace()) continue;

                if (ch == '"') return ExtractContinuousWordReverse(lineText, lineIndex, i);

                return null;
            }

            return ExtractCurrentPropertyName(lineIndex - 1, -1);
        }

        private PropertyNameLocation ExtractContinuousWordReverse(string lineText, int lineIndex, int lineOffset)
        {
            StringBuilder propertyNameBuilder = new StringBuilder();

            for (int i = lineOffset - 1; i >= 0; i--)
            {
                var ch = lineText[i];

                if (ch.IsPartOfWord())
                {
                    propertyNameBuilder.Append(ch);
                    continue;
                }

                if (ch == '"' && (i == 0 || lineText[i - 1] != '\\'))
                {
                    // начало propertyName
                    return new PropertyNameLocation(propertyNameBuilder.ToString().Reverse(), lineIndex, i);
                }

                return null;
            }

            // мы достигли начала строки, но так и не извлекли полноценный propertyName 
            return null;
        }

        private Direction? ChooseDirection(string selectedWord, int indexOfWord, string lineText)
        {
            var wordLength = selectedWord.Length;

            var lineLength = lineText.Length;

            // по сути проверка эквивалентна if (startBorderPos == -1 && endBorderPos == lineLength)
            if (wordLength == lineLength)
            {
                // selectedWord полностью занимает всю строку. значит это точно не stringValue (потому что нет кавычек по бокам)
                // значит это может быть только integerValue и ни что другое
                if (selectedWord.IsInteger()) return Direction.LEFT;
                return null;
            }

            int startBorderPos = indexOfWord - 1;

            int endBorderPos = indexOfWord + wordLength;

            if (startBorderPos == -1)
            {
                if (!lineText[endBorderPos].IsWhiteSpaceOr(',', '}', ']')) return null;
                if (selectedWord.IsInteger()) return Direction.LEFT;
                return null;
            }

            if (endBorderPos == lineLength)
            {
                if (!lineText[startBorderPos].IsWhiteSpaceOr(',', '[', ':')) return null;
                if (selectedWord.IsInteger()) return Direction.LEFT;
                return null;
            }

            var startBorderChar = lineText[startBorderPos];
            var endBorderChar = lineText[endBorderPos];

            if (startBorderChar == '"' && endBorderChar == '"')
            {
                // это либо propertyName, либо stringValue, но пойдем направо, думая что это первый вариант более вероятен
                return Direction.RIGHT;
            }

            if (startBorderChar == '"' || endBorderChar == '"')
            {
                // кавычка стоит только с одной стороны - это либо невалидный JSON, либо это строка с текстом из нескольких слов
                return null;
            }

            if (!selectedWord.IsInteger()) return null;

            if (startBorderChar.IsWhiteSpaceOr(',', '[', ':')
                && endBorderChar.IsWhiteSpaceOr(',', ']', '}'))
            {
                return Direction.LEFT;
            }

            return null;
        }

        // ожидается, что данный метод дергается, если мы наткнулись на двоеточие и теперь ожидаем встретить propertyValue
        private string ExtractPropertyValue(int lineIndex, int lineOffset)
        {
            if (lineIndex >= _totalLinesCount) return null;
            string lineText = GetLineText(lineIndex);

            int lineLength = lineText.Length;
            for (int i = lineOffset; i < lineLength; i++)
            {
                var ch = lineText[i];

                if (ch.IsWhiteSpace()) continue;

                if (ch == '"' || ch.IsDigitOrMinus())
                {
                    return ExtractContinuousWord(lineText, i);
                }

                // после двоеточия мы ожидаем либо string либо integer value
                return null;
            }

            return ExtractPropertyValue(lineIndex + 1, 0);
        }

        // @return:
        // - если это integer -> значение в формате -12345
        // - если это строка -> строку в формате <слово_которое_МОЖЕТ_быть_разделено__только_нижними_подчеркиваниями_и_содержать_цифры123_45> (без кавычек)
        // - null - если невалидный json, или строка, состоящая из нескольких слов, или начался новый объект '{' или массив '['
        private string ExtractContinuousWord(string lineText, int lineOffset)
        {
            var lineLength = lineText.Length;
            if (lineOffset >= lineLength)
            {
                // Logger.warn();
                return null;
            }

            char firstChar = lineText[lineOffset];
            bool isString = firstChar == '"';
            bool isInteger = !isString;

            StringBuilder valueBuilder = new StringBuilder();
            if (isInteger)
            {
                valueBuilder.Append(firstChar);
            }

            for (int i = lineOffset + 1; i < lineLength; i++)
            {
                var ch = lineText[i];

                if (ch.IsDigit())
                {
                    valueBuilder.Append(ch);
                    continue;
                }

                if (isString && (ch.IsLetter() || ch == '_'))
                {
                    valueBuilder.Append(ch);
                    continue;
                }

                if (isString && ch == '"')
                {
                    // конец stringValue
                    return valueBuilder.ToString();
                }

                if (isInteger && ch.IsWhiteSpaceOr(',', '}')) // конец массива быть не может:  <anything : integerValue]>
                {
                    // конец integerValue
                    return valueBuilder.ToString();
                }

                return null;
            }

            return isInteger
                ? valueBuilder.ToString() // конец integerValue
                : null; // строковое значение всегда должно заканчиватсья двойными кавычками и не может быть растянуто на несколько линий
        }


        //GetParentPropertyName: TODO: было бы неплохо возвращать null если json невалиден
        private PropertyNameLocation ExtractParentPropertyName(int initialLineIndex, int initialLineOffset)
        {
            var nestingCounter = 0;
            char? expectedChar = null;
            string propertyName = null;

            var lineOffset = initialLineOffset;

            for (int lineIndex = initialLineIndex; lineIndex >= 0; lineIndex--)
            {
                var lineText = GetLineText(lineIndex);
                if (lineIndex < initialLineIndex)
                {
                    lineOffset = lineText.Length - 1;
                }

                for (int charIndex = lineOffset; charIndex >= 0; charIndex--)
                {
                    char ch = lineText[charIndex];

                    if (ch.IsOneOf('}', ']'))
                    {
                        if (expectedChar == ':')
                        {
                            // unnamed parent found; try search next parent
                            nestingCounter = 0;
                            expectedChar = null;
                            continue;
                        }

                        nestingCounter--;
                        continue;
                    }

                    if (ch == expectedChar)
                    {
                        switch (expectedChar)
                        {
                            case ':':
                                expectedChar = '"';
                                break;
                            case '"' when propertyName == null:
                                // property start
                                propertyName = "";
                                break;
                            case '"':
                                // property end
                                return new PropertyNameLocation(propertyName, lineIndex, charIndex);
                        }

                        continue;
                    }

                    if (propertyName != null)
                    {
                        propertyName = ch + propertyName;
                        continue;
                    }

                    if (ch.IsOneOf('{', '['))
                    {
                        if (nestingCounter == 0)
                        {
                            expectedChar = ':';
                        }

                        nestingCounter++;
                    }
                }
            }

            return PropertyNameLocation.Root;
        }

        private bool IsValidLineIndex(int lineIndex)
        {
            return 0 <= lineIndex && lineIndex < _totalLinesCount;
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private enum Direction
        {
            LEFT,
            RIGHT
        }

        private class RightResult
        {
            internal readonly string PropertyValue;

            internal static RightResult GoLeft()
            {
                return new RightResult(null);
            }

            internal RightResult(string propertyValue)
            {
                PropertyValue = propertyValue;
            }

            internal bool IsNeedGoLeft()
            {
                return PropertyValue == null;
            }
        }
    }
}