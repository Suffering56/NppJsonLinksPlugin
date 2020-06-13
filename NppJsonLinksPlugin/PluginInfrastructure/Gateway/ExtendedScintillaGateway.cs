using System;
using System.Text;
using NppJsonLinksPlugin.Core;
using NppJsonLinksPlugin.Logic;

namespace NppJsonLinksPlugin.PluginInfrastructure.Gateway
{
    public partial class ScintillaGateway : IScintillaGateway
    {
        public void OpenFile(string filePath)
        {
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DOOPEN, 0, filePath);
        }

        public string GetFullCurrentPath()
        {
            StringBuilder sbPath = new StringBuilder(Win32.MAX_PATH);
            if ((int) Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETFULLCURRENTPATH, Win32.MAX_PATH, sbPath) == 1)
            {
                return sbPath.ToString();
            }

            return null;
        }

        public int PositionToLine(int position)
        {
            return (int) Win32.SendMessage(scintilla, SciMsg.SCI_LINEFROMPOSITION, position, 0);
        }

        public int LineToPosition(int line)
        {
            return (int) Win32.SendMessage(scintilla, SciMsg.SCI_POSITIONFROMLINE, line, 0);
        }

        public string GetTextFromPositionSafe(int startPosition, int length, int linesAdded)
        {
            try
            {
                return GetTextFromPosition(startPosition, length, linesAdded);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public string GetCurrentWord()
        {
            StringBuilder sbWord = new StringBuilder(4096);
            return Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETCURRENTWORD, 4096, sbWord) != IntPtr.Zero
                ? sbWord.ToString()
                : null;
        }

        public int GetCurrentLine()
        {
            return Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) NppMsg.NPPM_GETCURRENTLINE, 0, 0).ToInt32();
        }

        public string GetTextFromPosition(int startPosition, int length, int linesAdded)
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
                    // а еще эта проверка помогает с багой, когда введенный кириллический символ почему то в нотификации имеет length = 2 (поэтому мы и не можем тут бросить эксепшен)
                    return resultText;
                    throw new Exception($"an error occurred while extracting text from position, startPosition={startPosition}, length={length}");
                }

                var text = GetLineText(currentLineIndex);
                if (linesAdded == 0)
                {
                    text = text.TrimEnd('\r', '\n');
                }

                if (startOffset > 0)
                {
                    if (startOffset >= text.Length)
                    {
                        // NPP иногда и такое выдает (при чем такое происходит только на русской раскладке)
                        // я пока не могу объяснить такое поведение, так же как и менее костыльно его обработать
                        startOffset = text.Length - 1;
                    }

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

        public void JumpToLine(int line)
        {
            int currentPos = GetCurrentPos().Value;

            int currentLine = (int) Win32.SendMessage(scintilla, SciMsg.SCI_LINEFROMPOSITION, currentPos, 0);
            if (line != currentLine)
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_DOCUMENTEND, 0, 0);
                Win32.SendMessage(scintilla, SciMsg.SCI_ENSUREVISIBLEENFORCEPOLICY, line, 0);
                Win32.SendMessage(scintilla, SciMsg.SCI_GOTOLINE, line, 0);
            }

            Win32.SendMessage(scintilla, SciMsg.SCI_GRABFOCUS, 0, 0);
        }

        public JumpLocation GetCurrentLocation()
        {
            return new JumpLocation(GetFullCurrentPath(), GetCurrentLine());
        }
    }
}