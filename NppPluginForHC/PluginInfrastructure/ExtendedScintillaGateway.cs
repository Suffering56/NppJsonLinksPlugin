using System;

namespace NppPluginForHC.PluginInfrastructure
{
    public class ExtendedScintillaGateway : ScintillaGateway, IExtendedScintillaGateway
    {
        public ExtendedScintillaGateway(IntPtr scintilla) : base(scintilla)
        {
        }

        public int PositionToLine(int position)
        {
            return (int) Win32.SendMessage(scintilla, SciMsg.SCI_LINEFROMPOSITION, position, 0);
        }

        public int LineToPosition(int line)
        {
            return (int) Win32.SendMessage(scintilla, SciMsg.SCI_POSITIONFROMLINE, line, 0);
        }

        public string GetTextFromPosition(int startPosition, int length)
        {
            var line = PositionToLine(startPosition);
            var lineStartPosition = LineToPosition(line);

            var startOffset = startPosition - lineStartPosition;
            var lineOffset = 0;
            var resultText = "";

            while (resultText.Length < length)
            {
                var text = GetLine(line + lineOffset++);
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