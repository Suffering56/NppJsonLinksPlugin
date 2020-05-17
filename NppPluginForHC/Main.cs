using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using NppPluginForHC.Core;
using NppPluginForHC.Logic;
using NppPluginForHC.PluginInfrastructure;

namespace NppPluginForHC
{
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    internal static class Main
    {
        internal const string PluginName = "NppPluginForHC";
        private const string PluginVersion = "0.0.3";
        private static Settings _settings = null;

        private static readonly DefinitionSearchEngine SearchEngine = new DefinitionSearchEngine();
        private static bool _isPluginInited = false;
        private static bool _isFileLoadingActive = false;

        private static int _jumpPos = 0;
        private static readonly Stack<JumpLocation> JumpStack = new Stack<JumpLocation>();

        private static frmMyDlg _frmMyDlg = null;
        private static int _idMyDlg = -1;
        private static readonly Bitmap TbBmp = Properties.Resources.star;
        private static readonly Bitmap TbBmpTbTab = Properties.Resources.star_bmp;
        private static Icon _tbIcon = null;

        private static readonly EventHandler OnLeftMouseClick = delegate
        {
            if (Control.ModifierKeys == Keys.Control)
            {
                GoToDefinition();
            }
        };

        internal static void CommandMenuInit()
        {
            _idMyDlg = 1;
            
            PluginBase.SetCommand(0, "Version", () =>  MessageBox.Show($"Version: {PluginVersion}"), new ShortcutKey(false, false, false, Keys.None));
            // PluginBase.SetCommand(1, "MyDockableDialog", myDockableDialog);
            PluginBase.SetCommand(1, "", null);
            PluginBase.SetCommand(2, "GoToDefinition", GoToDefinition, new ShortcutKey(true, false, true, Keys.Enter));

            PluginBase.SetCommand(3, "Navigate Backward", NavigateBackward, new ShortcutKey(false, true, false, Keys.Left));
            PluginBase.SetCommand(4, "Navigate Forward", NavigateForward, new ShortcutKey(false, true, false, Keys.Right));
        }

        public static void OnNotification(ScNotification notification)
        {
            var notificationType = notification.Header.Code;

            // NPP successfully started
            if (notificationType == (uint) NppMsg.NPPN_READY)
            {
                ProcessInit();
                Logger.Info("NPPN_READY");
            }
            else if (_isPluginInited)
            {
                switch (notificationType)
                {
                    case (uint) NppMsg.NPPN_BUFFERACTIVATED:
                        // NPPN_BUFFERACTIVATED = switching tabs/open file/reload file/etc
                        SearchEngine.SwitchContext(GetCurrentFilePath());

                        Logger.Info($"NPPN_BUFFERACTIVATED: inited={_isPluginInited}");
                        break;

                    case (uint) SciMsg.SCN_FOCUSOUT:
                        // мы перестаем слушать клики мышкой, когда окно теряет фокус
                        MouseHook.CleanListeners();

                        Logger.Info("SCN_FOCUSOUT");
                        break;

                    case (uint) SciMsg.SCN_FOCUSIN:
                        // возобновляем слушание кликов мышкой, при получении фокуса
                        MouseHook.RegisterListener(OnLeftMouseClick);

                        Logger.Info("SCN_FOCUSIN");
                        break;

                    case (uint) NppMsg.NPPN_FILEBEFORELOAD:
                        // при загрузке файла происходит вызов SCN_MODIFIED, который мы должны игнорировать
                        _isFileLoadingActive = true;

                        Logger.Info("NPPN_FILEBEFORELOAD");
                        break;

                    case (uint) NppMsg.NPPN_FILEBEFOREOPEN: // or NppMsg.NPPN_FILEOPENED
                    case (uint) NppMsg.NPPN_FILELOADFAILED:
                        // файл загружен (возможно с ошибкой) и мы больше не должны игнорировать события SCN_MODIFIED
                        _isFileLoadingActive = false;

                        Logger.Info("NPPN_FILEBEFOREOPEN");
                        break;

                    case (uint) SciMsg.SCN_MODIFIED:
                        //TODO: почему-то на 64-битной версии NPP notification.ModificationType всегда = 0, поэтому пока все работает хорошо только на 32-битной
                        if (!_isFileLoadingActive)
                        {
                            ProcessModified(notification);
                            // Logger.Info("SCN_MODIFIED");
                        }

                        break;
                }
            }
        }

        private static void ProcessInit()
        {
            // загружаем настройки плагина
            _settings = LoadSettings();
            Logger.Info($"settings loaded: mappingFilePathPrefix={_settings.MappingFilePathPrefix}");

            // чтобы SCN_MODIFIED вызывался только, если был добавлен или удален текст
            PluginBase.GetGatewayFactory().Invoke().SetModEventMask((int) SciMsg.SC_MOD_INSERTTEXT | (int) SciMsg.SC_MOD_DELETETEXT);

            // NPPN_READY вызывается перед последним вызовом NPPN_BUFFERACTIVATED, поэтому нужно инициализировать SearchEngine
            SearchEngine.Init(_settings, GetCurrentFilePath());

            // инициализация обработчика кликов мышкой
            MouseHook.Start();
            MouseHook.RegisterListener(OnLeftMouseClick);

            // при запуске NPP вызывается миллиард событий, в том числе и интересующие нас NPPN_BUFFERACTIVATED, SCN_MODIFIED, etc. Но их не нужно обрабатывать до инициализации. 
            _isPluginInited = true;
        }

        private static void ProcessModified(ScNotification notification)
        {
            var isTextDeleted = (notification.ModificationType & ((int) SciMsg.SC_MOD_DELETETEXT)) > 0;
            var isTextInserted = (notification.ModificationType & ((int) SciMsg.SC_MOD_INSERTTEXT)) > 0;
            if (!isTextDeleted && !isTextInserted)
            {
                return;
            }

            var gateway = PluginBase.GetGatewayFactory().Invoke();
            // количество строк, которые были добавлены/удалены (если отрицательное)
            int linesAdded = notification.LinesAdded;
            // глобальная позиция каретки, ДО вставки текста
            int currentPosition = notification.Position.Value;
            // строка, в которую вставили текст
            int currentLine = gateway.PositionToLine(currentPosition);
            // чтобы было удобнее смотреть в NPP
            const int viewLineOffset = 1;

            if (isTextInserted)
            {
                var insertedText = gateway.GetTextFromPositionSafe(currentPosition, notification.Length, notification.LinesAdded);
                // SearchEngine.FireInsertText(currentLine, insertedText);
                Logger.Info($"SCN_MODIFIED: Insert[{currentLine + viewLineOffset},{currentLine + viewLineOffset + linesAdded}], text:\r\n<{insertedText}>");
            }

            if (isTextDeleted)
            {
                // SearchEngine.FireDeleteText(currentLine, insertedText);
                if (linesAdded < 0)
                {
                    Logger.Info($"SCN_MODIFIED:Delete: from: {currentLine + viewLineOffset + 1} to: {currentLine + viewLineOffset - linesAdded}");
                }
                else
                {
                    Logger.Info($"SCN_MODIFIED:Delete: from: {currentLine + viewLineOffset}");
                }
            }
        }

        private static Settings LoadSettings()
        {
            try
            {
                return SettingsParser.Parse($"plugins/{Main.PluginName}/settings.json");
            }
            catch (Exception e)
            {
                Logger.Error("LoadSettings exception: " + e.GetType());
                Logger.Error(e);
                throw;
            }
        }


        internal static void OnShutdown()
        {
            MouseHook.CleanListeners();
            MouseHook.Stop();
            // Win32.WritePrivateProfileString("SomeSection", "SomeKey", someSetting ? "1" : "0", iniFilePath);
        }

        private static void GoToDefinition()
        {
            var gateway = PluginBase.GetGatewayFactory().Invoke();
            string selectedWord = gateway.GetCurrentWord();

            if (!string.IsNullOrEmpty(selectedWord))
            {
                JumpLocation jumpLocation = SearchEngine.FindDefinitionLocation(selectedWord, () => new SearchContext(gateway));

                if (jumpLocation != null)
                {
                    ShowLineInNotepadPP(jumpLocation);
                }
                else
                {
                    Logger.Info($"definition not found for word: {selectedWord}");
                    System.Media.SystemSounds.Asterisk.Play();
                }

                return;
            }

            Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_GRABFOCUS, 0, 0);
            System.Media.SystemSounds.Hand.Play();
        }

        private static string GetCurrentFilePath()
        {
            StringBuilder sbPath = new StringBuilder(Win32.MAX_PATH);
            if ((int) Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETFULLCURRENTPATH, Win32.MAX_PATH, sbPath) == 1)
            {
                return sbPath.ToString();
            }

            Logger.Error("Current time path not found?");
            throw new Exception("Current time path not found?");
        }

        private static void ShowLineInNotepadPP(JumpLocation jumpLocation)
        {
            PushJump(jumpLocation.FilePath, jumpLocation.Line);
            ShowLineInNotepadPP(jumpLocation.FilePath, jumpLocation.Line);
        }

        private static void ShowLineInNotepadPP(string file, int line)
        {
            Logger.Info($"Opening file '{file}'");
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DOOPEN, 0, file);
            IntPtr curScintilla = PluginBase.GetCurrentScintilla();

            Utils.ExecuteDelayed(() => // задержка фиксит багу с выделением текста при переходе
            {
                int currentPos = (int) Win32.SendMessage(curScintilla, SciMsg.SCI_GETCURRENTPOS, 0, 0);

                int currentLine = (int) Win32.SendMessage(curScintilla, SciMsg.SCI_LINEFROMPOSITION, currentPos, 0);
                if ((line != 1) && (line - 1 != currentLine))
                {
                    Win32.SendMessage(curScintilla, SciMsg.SCI_DOCUMENTEND, 0, 0);
                    Win32.SendMessage(curScintilla, SciMsg.SCI_ENSUREVISIBLEENFORCEPOLICY, line - 1, 0);
                    Win32.SendMessage(curScintilla, SciMsg.SCI_GOTOLINE, line - 1, 0);
                }

                Win32.SendMessage(curScintilla, SciMsg.SCI_GRABFOCUS, 0, 0);
            }, 100);
        }

        internal static void PushJump(string source, int line)
        {
            StringBuilder sbPath = new StringBuilder(Win32.MAX_PATH);
            if ((int) Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETFULLCURRENTPATH, Win32.MAX_PATH, sbPath) == 1)
            {
                var currentFilePath = sbPath.ToString();

                if ((currentFilePath != "") && File.Exists(currentFilePath))
                {
                    var currentLine = (int) Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETCURRENTLINE, 0, 0) + 1;
                    JumpLocation jumpOldPos = new JumpLocation(currentFilePath, currentLine);

                    while (((JumpStack.Count) > _jumpPos)
                           || (((JumpStack.Count > 0))
                               && (JumpStack.Peek().FilePath == jumpOldPos.FilePath)
                               && (JumpStack.Peek().Line == jumpOldPos.Line)))
                        JumpStack.Pop();
                    JumpStack.Push(jumpOldPos);
                    JumpStack.Push(new JumpLocation(source, line));
                    _jumpPos = JumpStack.Count;
                }
            }
        }

        static void NavigateBackward()
        {
            try
            {
                if ((_jumpPos <= 1)) throw new Exception();
                int newPos = JumpStack.Count - (--_jumpPos);
                ShowLineInNotepadPP(JumpStack.ToArray()[newPos]);
            }
            catch
            {
                System.Media.SystemSounds.Hand.Play();
            }
        }

        static void NavigateForward()
        {
            try
            {
                if (JumpStack.Count <= _jumpPos) throw new Exception();
                int newPos = JumpStack.Count - (++_jumpPos);
                ShowLineInNotepadPP(JumpStack.ToArray()[newPos]);
            }
            catch
            {
                System.Media.SystemSounds.Hand.Play();
            }
        }


        #region " Layout Base "

        internal static void myDockableDialog()
        {
            if (_frmMyDlg == null)
            {
                _frmMyDlg = new frmMyDlg();
        
                using (Bitmap newBmp = new Bitmap(16, 16))
                {
                    Graphics g = Graphics.FromImage(newBmp);
                    ColorMap[] colorMap = new ColorMap[1];
                    colorMap[0] = new ColorMap();
                    colorMap[0].OldColor = Color.Fuchsia;
                    colorMap[0].NewColor = Color.FromKnownColor(KnownColor.ButtonFace);
                    ImageAttributes attr = new ImageAttributes();
                    attr.SetRemapTable(colorMap);
                    g.DrawImage(TbBmpTbTab, new Rectangle(0, 0, 16, 16), 0, 0, 16, 16, GraphicsUnit.Pixel, attr);
                    _tbIcon = Icon.FromHandle(newBmp.GetHicon());
                }
        
                NppTbData _nppTbData = new NppTbData();
                _nppTbData.hClient = _frmMyDlg.Handle;
                _nppTbData.pszName = "My dockable dialog";
                _nppTbData.dlgID = _idMyDlg;
                _nppTbData.uMask = NppTbMsg.DWS_DF_CONT_RIGHT | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR;
                _nppTbData.hIconTab = (uint) _tbIcon.Handle;
                _nppTbData.pszModuleName = PluginName;
                IntPtr _ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(_nppTbData));
                Marshal.StructureToPtr(_nppTbData, _ptrNppTbData, false);
        
                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) NppMsg.NPPM_DMMREGASDCKDLG, 0, _ptrNppTbData);
            }
        
            else
            {
                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) NppMsg.NPPM_DMMSHOW, 0, _frmMyDlg.Handle);
            }
        }

        internal static void SetToolBarIcon()
        {
            toolbarIcons tbIcons = new toolbarIcons();
            tbIcons.hToolbarBmp = TbBmp.GetHbitmap();
            IntPtr pTbIcons = Marshal.AllocHGlobal(Marshal.SizeOf(tbIcons));
            Marshal.StructureToPtr(tbIcons, pTbIcons, false);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) NppMsg.NPPM_ADDTOOLBARICON, PluginBase._funcItems.Items[_idMyDlg]._cmdID, pTbIcons);
            Marshal.FreeHGlobal(pTbIcons);
        }

        #endregion " Layout Base "
    }
}