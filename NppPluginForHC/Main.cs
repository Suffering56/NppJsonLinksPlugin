using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
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
        internal static Settings Settings = null;

        private static readonly DefinitionSearchEngine SearchEngine = new DefinitionSearchEngine();
        private static bool _isPluginInited = false;
        private static bool _isFileLoadingActive = false;

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

        private static void GetVersion()
        {
            MessageBox.Show("Version: 0.0.2");
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
                        }

                        break;
                }
            }
        }

        private static void ProcessInit()
        {
            // загружаем настройки плагина
            Settings = LoadSettings();
            Logger.Info($"settings loaded: mappingFilePathPrefix={Settings.MappingFilePathPrefix}");

            // чтобы SCN_MODIFIED вызывался только, если был добавлен или удален текст
            PluginBase.GetGatewayFactory().Invoke().SetModEventMask((int) SciMsg.SC_MOD_INSERTTEXT | (int) SciMsg.SC_MOD_DELETETEXT);

            // NPPN_READY вызывается перед последним вызовом NPPN_BUFFERACTIVATED, поэтому нужно инициализировать SearchEngine
            SearchEngine.Init(Settings, GetCurrentFilePath());

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
                var insertedText = gateway.GetTextFromPositionSafe(currentPosition, notification.Length);
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

        internal static void CommandMenuInit()
        {
            PluginBase.SetCommand(0, "Version", GetVersion, new ShortcutKey(false, false, false, Keys.None));
            PluginBase.SetCommand(1, "MyDockableDialog", myDockableDialog);
            _idMyDlg = 1;
            PluginBase.SetCommand(2, "GoToDefinition", GoToDefinition, new ShortcutKey(true, false, true, Keys.Enter));
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


        static int jumpPos =
            0;

        struct Jump
        {
            public string FilePath;
            public int Line;
        }

        static Stack<Jump> jumpStack = new Stack<Jump>();

        // static void GoToDefinition()
        // {
        //     TRACE("-START-");
        //     try
        //     {
        //         StringBuilder sbWord = new StringBuilder(4096);
        //         if (Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETCURRENTWORD, 4096, sbWord) != IntPtr.Zero)
        //         {
        //             String word = sbWord.ToString();
        //             List<Tag> lstMatches = new List<Tag>();
        //             List<QuickTag> lstMatchesQ = new List<QuickTag>();
        //
        //             if (frmMain == null)
        //             {
        //                 ShowFrmMain();
        //                 Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DMMHIDE, 0, frmMain.Handle);
        //             }
        //             else if (!frmMain.Visible)
        //             {
        //                 if (Settings.Configs.SessionMode == Settings.SessionMode.None)
        //                     frmMain.DoCurrentSciBufferTags();
        //                 else if (Settings.Configs.SessionMode == Settings.SessionMode.Npp)
        //                     frmMain.DoAllOpenedDocuments();
        //             }
        //
        //             int numSources = 0;
        //             TRACE(string.Format("Searching word '{0}'", sbWord));
        //             if (sbWord.Length > 0)
        //             {
        //                 foreach (TreeNode sourceNode in frmMain.tvTags.Nodes)
        //                 {
        //                     bool found = false;
        //                     if ((Settings.Configs.SessionMode == Settings.SessionMode.Cookie)
        //                         && (sourceNode.Name == Source.INCLUDES))
        //                     {
        //                         foreach (IncludeFile incFile in sourceNode.Nodes)
        //                         {
        //                             if (incFile.QuickTags == null) continue;
        //                             bool _found = false;
        //                             foreach (QuickTag qtag in incFile.QuickTags)
        //                                 if (string.Compare(qtag.Text, word, !Settings.Languages[qtag.Language].CaseSensitive) == 0)
        //                                 {
        //                                     lstMatchesQ.Add(qtag);
        //                                     _found = true;
        //                                 }
        //
        //                             if (_found) numSources++;
        //                         }
        //                     }
        //                     else
        //                     {
        //                         Source source = (Source) sourceNode.Tag;
        //                         foreach (string tagType in source.TagTypes.Keys)
        //                         foreach (Tag tag in source.TagTypes[tagType].Tags)
        //                             if (string.Compare(tag.TagName, word, !Settings.Languages[tag.Language].CaseSensitive) == 0)
        //                             {
        //                                 lstMatches.Add(tag);
        //                                 found = true;
        //                             }
        //                     }
        //
        //                     if (found) numSources++;
        //                 }
        //
        //                 foreach (TreeNode sourceNode in Source.invisibleSourceNodes)
        //                 {
        //                     bool found = false;
        //                     Source source = (Source) sourceNode.Tag;
        //                     foreach (string tagType in source.TagTypes.Keys)
        //                     foreach (Tag tag in source.TagTypes[tagType].Tags)
        //                         if (string.Compare(tag.TagName, word, !Settings.Languages[tag.Language].CaseSensitive) == 0)
        //                         {
        //                             lstMatches.Add(tag);
        //                             found = true;
        //                         }
        //
        //                     if (found) numSources++;
        //                 }
        //
        //                 foreach (IncludeFile incFile in Source.invisibleIncludeFileNodes)
        //                 {
        //                     if (incFile.QuickTags == null) continue;
        //                     bool _found = false;
        //                     foreach (QuickTag qtag in incFile.QuickTags)
        //                         if (string.Compare(qtag.Text, word, !Settings.Languages[qtag.Language].CaseSensitive) == 0)
        //                         {
        //                             lstMatchesQ.Add(qtag);
        //                             _found = true;
        //                         }
        //
        //                     if (_found) numSources++;
        //                 }
        //             }
        //
        //             TRACE(string.Format("Word found {0} times", numSources));
        //
        //             if ((lstMatches.Count + lstMatchesQ.Count) > 256)
        //             {
        //                 if (MessageBox.Show("You will get more than 256 matches.\nGoing on with the search might \n"
        //                                     + "deadlock the Notepad++ process.\nDo you want to cancel the search?",
        //                         "SourceCookifier", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
        //                     == DialogResult.Yes) return;
        //                 TRACE("Ignoring 256 matches limit");
        //             }
        //
        //             if ((lstMatches.Count > 0) || (lstMatchesQ.Count > 0))
        //             {
        //                 TRACE(string.Format("lstMatches.Count={0} lstMatchesQ.Count={1}", lstMatches.Count, lstMatchesQ.Count));
        //                 if ((lstMatches.Count == 1) && (lstMatchesQ.Count == 0))
        //                 {
        //                     PushJump(lstMatches[0].SourceFile, lstMatches[0].Line);
        //                     ShowLineInNotepadPP(lstMatches[0].SourceFile, lstMatches[0].Line, true);
        //                 }
        //                 else if ((lstMatchesQ.Count == 1) && (lstMatches.Count == 0))
        //                 {
        //                     PushJump(lstMatchesQ[0].SourceFile, lstMatchesQ[0].Line);
        //                     ShowLineInNotepadPP(lstMatchesQ[0].SourceFile, lstMatchesQ[0].Line, true);
        //                 }
        //                 else
        //                 {
        //                     string currentSourceFile = "";
        //                     StringBuilder sbPath = new StringBuilder(Win32.MAX_PATH);
        //                     if ((int) Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETFULLCURRENTPATH, Win32.MAX_PATH, sbPath) == 1)
        //                         currentSourceFile = sbPath.ToString();
        //
        //                     if (cmsDefinitions == null)
        //                     {
        //                         cmsDefinitions = new ContextMenuStrip();
        //                         cmsDefinitions.ItemClicked += new ToolStripItemClickedEventHandler(cmsDefinitions_ItemClicked);
        //                         cmsDefinitions.ImageList = Settings.IconList;
        //                     }
        //                     else
        //                     {
        //                         cmsDefinitions.Items.Clear();
        //                     }
        //
        //                     Font fontBold = new Font(cmsDefinitions.Font.FontFamily, cmsDefinitions.Font.Size, FontStyle.Bold);
        //                     Font fontItalic = new Font(cmsDefinitions.Font.FontFamily, cmsDefinitions.Font.Size, FontStyle.Italic);
        //                     Font fontItalicBold = new Font(cmsDefinitions.Font.FontFamily, cmsDefinitions.Font.Size, FontStyle.Italic | FontStyle.Bold);
        //
        //                     int numInserts = 0;
        //                     foreach (Tag tag in lstMatches)
        //                     {
        //                         string path = "";
        //                         if (tag.SourceFile != currentSourceFile)
        //                         {
        //                             if (Settings.Configs.ShowGtdToolTips)
        //                                 path = " - " + Path.GetFileName(tag.SourceFile);
        //                             else
        //                                 path = " - " + tag.SourceFile;
        //                         }
        //
        //                         ToolStripMenuItem tsmi = new ToolStripMenuItem(string.Format("{0} [{1}]{2}",
        //                             (tag.ToolTipText != "") ? tag.ToolTipText : tag.Text, tag.Line, path));
        //
        //                         if (Settings.Configs.ShowGtdToolTips)
        //                             tsmi.ToolTipText = tag.SourceFile;
        //                         tsmi.ImageIndex = Settings.Languages[tag.Language].TagTypes[tag.Identifier].ImageListIndex;
        //                         tsmi.ForeColor = Color.FromArgb(Settings.Languages[tag.Language].TagTypes[tag.Identifier].ForeColor);
        //                         tsmi.Tag = tag;
        //                         if (tag.SourceFile == currentSourceFile)
        //                         {
        //                             tsmi.Font = fontBold;
        //                             cmsDefinitions.Items.Insert(numInserts++, tsmi);
        //                         }
        //                         else
        //                         {
        //                             cmsDefinitions.Items.Add(tsmi);
        //                         }
        //                     }
        //
        //                     foreach (QuickTag qtag in lstMatchesQ)
        //                     {
        //                         string path = "";
        //                         if (qtag.SourceFile != currentSourceFile)
        //                         {
        //                             if (Settings.Configs.ShowGtdToolTips)
        //                                 path = " - " + Path.GetFileName(qtag.SourceFile);
        //                             else
        //                                 path = " - " + qtag.SourceFile;
        //                         }
        //
        //                         ToolStripMenuItem tsmi = new ToolStripMenuItem(string.Format("{0} [{1}]{2}",
        //                             qtag.FullText, qtag.Line, path));
        //
        //                         if (Settings.Configs.ShowGtdToolTips)
        //                             tsmi.ToolTipText = qtag.SourceFile;
        //                         tsmi.ImageIndex = Settings.Languages[qtag.Language].TagTypes[qtag.Identifier].ImageListIndex;
        //                         tsmi.ForeColor = Color.FromArgb(Settings.Languages[qtag.Language].TagTypes[qtag.Identifier].ForeColor);
        //                         tsmi.Tag = qtag;
        //                         if (qtag.SourceFile == currentSourceFile)
        //                         {
        //                             tsmi.Font = fontItalicBold;
        //                             cmsDefinitions.Items.Insert(numInserts++, tsmi);
        //                         }
        //                         else
        //                         {
        //                             tsmi.Font = fontItalic;
        //                             cmsDefinitions.Items.Add(tsmi);
        //                         }
        //                     }
        //
        //                     if ((numInserts > 0) && (numInserts < cmsDefinitions.Items.Count))
        //                     {
        //                         cmsDefinitions.Items.Insert(numInserts, new ToolStripSeparator());
        //                     }
        //
        //                     IntPtr curSci = PluginBase.GetCurrentScintilla();
        //                     int currentPos = (int) Win32.SendMessage(curSci, SciMsg.SCI_GETCURRENTPOS, 0, 0);
        //                     Point pt = new Point();
        //                     pt.X = (int) Win32.SendMessage(curSci, SciMsg.SCI_POINTXFROMPOSITION, 0, currentPos);
        //                     pt.Y = (int) Win32.SendMessage(curSci, SciMsg.SCI_POINTYFROMPOSITION, 0, currentPos);
        //                     Win32.ClientToScreen(curSci, ref pt);
        //                     TRACE(string.Format("Showing cmsDefinitions menu at X={0} Y={1}", pt.X, pt.Y));
        //                     cmsDefinitions.Show(pt);
        //                 }
        //             }
        //             else
        //             {
        //                 Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_GRABFOCUS, 0, 0);
        //                 System.Media.SystemSounds.Hand.Play();
        //             }
        //         }
        //         else
        //         {
        //             TRACE("NPPM_GETCURRENTWORD failed");
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         ErrorOut(ex);
        //     }
        //
        //     TRACE("-END-");
        // }

        static void cmsDefinitions_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            // TRACE("-START-");
            // try
            // {
            //     if (e.ClickedItem.Tag is Tag)
            //     {
            //         Tag tag = e.ClickedItem.Tag as Tag;
            //         PushJump(tag.SourceFile, tag.Line);
            //         ShowLineInNotepadPP(tag.SourceFile, tag.Line, true);
            //     }
            //     else if (e.ClickedItem.Tag is QuickTag)
            //     {
            //         QuickTag qtag = e.ClickedItem.Tag as QuickTag;
            //         PushJump(qtag.SourceFile, qtag.Line);
            //         ShowLineInNotepadPP(qtag.SourceFile, qtag.Line, true);
            //     }
            // }
            // catch (Exception ex)
            // {
            //     ErrorOut(ex);
            // }
            //
            // TRACE("-END-");
        }

        internal static void PushJump(string source, int line)
        {
            // TRACE("-START-");
            // IntPtr curScintilla = PluginBase.GetCurrentScintilla();
            // StringBuilder sbPath = new StringBuilder(Win32.MAX_PATH);
            // if ((int) Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETFULLCURRENTPATH, Win32.MAX_PATH, sbPath) == 1)
            // {
            //     Jump jumpNewPos = new Jump();
            //     jumpNewPos.FilePath = source;
            //     jumpNewPos.Line = line;
            //     Jump jumpOldPos = new Jump();
            //     jumpOldPos.FilePath = sbPath.ToString();
            //     if ((jumpOldPos.FilePath != "") && File.Exists(jumpOldPos.FilePath))
            //     {
            //         jumpOldPos.Line = (int) Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETCURRENTLINE, 0, 0) + 1;    
            //         while (((jumpStack.Count) > jumpPos)
            //                || (((jumpStack.Count > 0))
            //                    && (jumpStack.Peek().FilePath == jumpOldPos.FilePath)
            //                    && (jumpStack.Peek().Line == jumpOldPos.Line)))
            //             jumpStack.Pop();
            //         jumpStack.Push(jumpOldPos);
            //         jumpStack.Push(jumpNewPos);
            //         jumpPos = jumpStack.Count;
            //     }
            // }
            //
            // TRACE("-END-");
        }

        private static void ShowLineInNotepadPP(JumpLocation jumpLocation)
        {
            ShowLineInNotepadPP(jumpLocation.FilePath, jumpLocation.Line);
        }

        private static void ShowLineInNotepadPP(string file, int line)
        {
            Logger.Info($"Opening file '{file}'");
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DOOPEN, 0, file);
            IntPtr curScintilla = PluginBase.GetCurrentScintilla();

            int currentPos = (int) Win32.SendMessage(curScintilla, SciMsg.SCI_GETCURRENTPOS, 0, 0);

            int currentLine = (int) Win32.SendMessage(curScintilla, SciMsg.SCI_LINEFROMPOSITION, currentPos, 0);
            if ((line != 1) && (line - 1 != currentLine))
            {
                Win32.SendMessage(curScintilla, SciMsg.SCI_DOCUMENTEND, 0, 0);
                Win32.SendMessage(curScintilla, SciMsg.SCI_ENSUREVISIBLEENFORCEPOLICY, line - 1, 0);
                Win32.SendMessage(curScintilla, SciMsg.SCI_GOTOLINE, line - 1, 0);
            }

            Win32.SendMessage(curScintilla, SciMsg.SCI_GRABFOCUS, 0, 0);
        }

        static void NavigateBackward()
        {
            // TRACE("-START-");
            // try
            // {
            //     if ((frmMain == null) || (jumpPos <= 1)) throw new Exception();
            //     int newPos = jumpStack.Count - (--jumpPos);
            //     ShowLineInNotepadPP(jumpStack.ToArray()[newPos].FilePath, jumpStack.ToArray()[newPos].Line, true);
            // }
            // catch
            // {
            //     System.Media.SystemSounds.Hand.Play();
            // }
            //
            // TRACE("-END-");
        }

        static void NavigateForward()
        {
            // TRACE("-START-");
            // try
            // {
            //     if ((jumpStack.Count) <= jumpPos) throw new Exception();
            //     int newPos = jumpStack.Count - (++jumpPos);
            //     ShowLineInNotepadPP(jumpStack.ToArray()[newPos].FilePath, jumpStack.ToArray()[newPos].Line, true);
            // }
            // catch
            // {
            //     System.Media.SystemSounds.Hand.Play();
            // }
            //
            // TRACE("-END-");
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