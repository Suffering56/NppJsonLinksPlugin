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

        public void SetIndicatorStyle(int indicatorId, int indicatorStyle)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_INDICSETSTYLE, indicatorId, indicatorStyle);
        }

        public void ApplyIndicatorStyleForRange(int indicatorId, int startPosition, int length)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETINDICATORCURRENT, indicatorId, Unused);
            Win32.SendMessage(scintilla, SciMsg.SCI_INDICATORFILLRANGE, startPosition, length);
        }

        public void ClearIndicatorStyleForRange(int indicatorId, int startPosition, int length)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETINDICATORCURRENT, indicatorId, Unused);
            Win32.SendMessage(scintilla, SciMsg.SCI_INDICATORCLEARRANGE, startPosition, length);
        }

        public int IndexPositionFromLine(int lineIndex, int linePosition)
        {
            return (int) Win32.SendMessage(scintilla, SciMsg.SCI_INDEXPOSITIONFROMLINE, lineIndex, linePosition);
        }
    }
}