using System;

namespace NppPluginForHC.PluginInfrastructure
{
    public class ExtendedScintillaGateway : ScintillaGateway, IExtendedScintillaGateway
    {
        public ExtendedScintillaGateway(IntPtr scintilla) : base(scintilla)
        {
            // do nothing: all right
        }

        public int PositionToLine(int position)
        {
            return (int) Win32.SendMessage(scintilla, SciMsg.SCI_LINEFROMPOSITION, position, 0);
        }

        public int LineToPosition(int line)
        {
            return (int) Win32.SendMessage(scintilla, SciMsg.SCI_POSITIONFROMLINE, line, 0);
        }

        public string GetTextFromPositionSafe(int startPosition, int length)
        {
            try
            {
                return GetTextFromPosition(startPosition, length);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public string GetTextFromPosition(int startPosition, int length)
        {
            var initialLineIndex = PositionToLine(startPosition);
            var lineStartPosition = LineToPosition(initialLineIndex);
            var totalLinesCount = GetLineCount();

            var startOffset = startPosition - lineStartPosition;
            var lineIndexOffset = 0;
            var resultText = "";

            while (resultText.Length < length)
            {
                int currentLineIndex = initialLineIndex + lineIndexOffset++;
                if (currentLineIndex >= totalLinesCount)
                {
                    // без этой проверки - можно словить зависание программы.
                    throw new Exception($"an error occurred while extracting text from position, startPosition={startPosition}, length={length}");
                }

                var text = GetLineText(currentLineIndex);
                if (startOffset > 0)
                {
                    text = text.Substring(startOffset, text.Length - startOffset);
                    startOffset = 0;
                }

                var endOffset = (resultText.Length + text.Length) - length;
                if (endOffset > 0)
                {
                    text = text.Substring(0, text.Length - endOffset);
                }

                resultText += text;
            }

            return resultText;
        }
    }
}