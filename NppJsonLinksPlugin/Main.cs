using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using NppJsonLinksPlugin.Configuration;
using NppJsonLinksPlugin.Core;
using NppJsonLinksPlugin.Forms;
using NppJsonLinksPlugin.Logic;
using NppJsonLinksPlugin.Logic.Context;
using NppJsonLinksPlugin.PluginInfrastructure;
using NppJsonLinksPlugin.PluginInfrastructure.Gateway;

namespace NppJsonLinksPlugin
{
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    internal static class Main
    {
        internal const string PluginName = "NppJsonLinksPlugin";
        private const string PluginVersion = "0.2.1";

        private static readonly IniConfig Config = new IniConfig();
        private static Settings _settings = null;

        private static readonly Func<string, IScintillaGateway, ISearchContext> SearchContextFactory = (clickedWord, gateway) => new JsonSearchContext(clickedWord, gateway);
        private static readonly SearchEngine SearchEngine = new SearchEngine();

        //TODO многовато флагов
        private static bool _isPluginInited = false;
        private static bool _isPluginDisabled = false; //TODO: unsupported
        private static bool _isFileLoadingActive = false;

        private static int _jumpPos = 0;
        private static readonly Stack<JumpLocation> JumpStack = new Stack<JumpLocation>();

        private static frmMyDlg _frmMyDlg = null;
        private static int _idMyDlg = 1;
        private static readonly Bitmap TbBmp = Properties.Resources.star;
        private static readonly Bitmap TbBmpTbTab = Properties.Resources.star_bmp;
        private static Icon _tbIcon = null;

        public static void DisablePlugin()
        {
            _isPluginDisabled = true;
            Logger.SetMode(Logger.Mode.DISABLED, null);
            MouseHook.Stop();
            MouseHook.CleanListeners();
        }

        private static readonly EventHandler OnLeftMouseClick = delegate
        {
            if (Control.ModifierKeys == Keys.Control)
            {
                GoToDefinition();
            }
        };

        internal static void CommandMenuInit()
        {
            Logger.Info("COMMAND_MENU_INIT");
            LoadIniConfig();
            // PluginBase.SetCommand(1, "MyDockableDialog", myDockableDialog);

            PluginBase.SetCommand(1, "Reload plugin", ReloadPlugin, new ShortcutKey(true, false, false, Keys.F5));
            PluginBase.SetCommand(2, "", null);

            PluginBase.SetCommand(3, "GoToDefinition", GoToDefinition, new ShortcutKey(true, false, true, Keys.Enter));
            PluginBase.SetCommand(4, "Navigate Backward", NavigateBackward, new ShortcutKey(true, true, false, Keys.Left));
            PluginBase.SetCommand(5, "Navigate Forward", NavigateForward, new ShortcutKey(true, true, false, Keys.Right));
            PluginBase.SetCommand(6, "", null);

            PluginBase.SetCommand(7, "Version", () => MessageBox.Show($@"Version: {PluginVersion}"), new ShortcutKey(false, false, false, Keys.None));
        }

        private static void LoadIniConfig()
        {
            Config.Load();
            Logger.SetMode(Config.LoggerMode, Config.LogsDir);
        }

        private static void ReloadPlugin()
        {
            MouseHook.Stop();
            MouseHook.CleanListeners();

            MouseHook.Start();
            MouseHook.RegisterListener(OnLeftMouseClick);
        }

        public static void OnNotification(ScNotification notification)
        {
            if (_isPluginDisabled)
            {
                //TODO: подумать о том, что человек захочет включить плагин ручками и это нужно отловить
                return;
            }

            var notificationType = notification.Header.Code;

            if (notificationType == (uint) NppMsg.NPPN_READY)
            {
                ProcessInit();
                Logger.Info("NPPN_READY");
                return;
            }

            if (!_isPluginInited) return;

            switch (notificationType)
            {
                case (uint) NppMsg.NPPN_BUFFERACTIVATED:
                    // NPPN_BUFFERACTIVATED = switching tabs/open file/reload file/etc
                    var gateway = PluginBase.GetGatewayFactory().Invoke();
                    SearchEngine.SwitchContext(gateway.GetFullCurrentPath());

                    Logger.Info($"NPPN_BUFFERACTIVATED");
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

                case (uint) SciMsg.SCN_SAVEPOINTREACHED:
                    // пользователь сохранил изменения в текущем файле (ctrl + s)
                    SearchEngine.FireSaveFile();
                    Logger.Info("SCN_SAVEPOINTREACHED");
                    break;

                case (uint) SciMsg.SCN_MODIFIED:
                    //TODO: почему-то на 64-битной версии NPP notification.ModificationType всегда = 0, поэтому пока все работает хорошо только на 32-битной
                    if (!_isFileLoadingActive)
                    {
                        // при отключенном кэше SearchEngine - нам не нужно отслеживать вставленный/удаленный текст
                        if (!_settings.CacheEnabled) return;

                        ProcessModified(notification);
                    }

                    break;
            }
        }

        private static void ProcessInit()
        {
            try
            {
                var gateway = PluginBase.GetGatewayFactory().Invoke();

                // загружаем настройки плагина
                _settings = LoadSettings();
                Logger.Info($"settings loaded: mappingFilePathPrefix={_settings.MappingDefaultFilePath}");

                // чтобы SCN_MODIFIED вызывался только, если был добавлен или удален текст
                gateway.SetModEventMask((int) SciMsg.SC_MOD_INSERTTEXT | (int) SciMsg.SC_MOD_DELETETEXT);

                // NPPN_READY вызывается перед последним вызовом NPPN_BUFFERACTIVATED, поэтому нужно инициализировать SearchEngine
                SearchEngine.Init(_settings, gateway.GetFullCurrentPath());

                // инициализация обработчика кликов мышкой
                MouseHook.Start();
                MouseHook.RegisterListener(OnLeftMouseClick);

                // при запуске NPP вызывается миллиард событий, в том числе и интересующие нас NPPN_BUFFERACTIVATED, SCN_MODIFIED, etc. Но их не нужно обрабатывать до инициализации. 
                _isPluginInited = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
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
                var insertedText = gateway.GetTextFromPositionSafe(currentPosition, notification.Length, linesAdded);
                SearchEngine.FireInsertText(currentLine, linesAdded, insertedText);
                Logger.Info($"SCN_MODIFIED: Insert[{currentLine + viewLineOffset},{currentLine + viewLineOffset + linesAdded}], text:\r\n<{insertedText}>");
            }

            if (isTextDeleted)
            {
                SearchEngine.FireDeleteText(currentLine, -linesAdded);
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
                return SettingsParser.Load(Config);
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
                JumpLocation jumpLocation = SearchEngine.FindDefinitionLocation(SearchContextFactory.Invoke(selectedWord, gateway));

                if (jumpLocation != null)
                {
                    JumpToLocation(jumpLocation);
                    return;
                }
            }

            gateway.GrabFocus();

            if (_settings.SoundEnabled)
            {
                System.Media.SystemSounds.Asterisk.Play();
            }
        }

        private static void JumpToLocation(JumpLocation jumpLocation)
        {
            PushJump(jumpLocation.FilePath, jumpLocation.Line);

            string file = jumpLocation.FilePath;
            int line = jumpLocation.Line;

            Logger.Info($"Opening file '{file}'");

            var gateway = PluginBase.GetGatewayFactory().Invoke();
            gateway.OpenFile(file);

            // задержка фиксит багу с выделением текста при переходе
            ThreadSupport.ExecuteDelayed(() => gateway.JumpToLine(line), _settings.JumpToLineDelay);
        }

        private static void PushJump(string source, int line)
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
                JumpToLocation(JumpStack.ToArray()[newPos]);
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
                JumpToLocation(JumpStack.ToArray()[newPos]);
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