using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
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
        internal const string PLUGIN_NAME = "NppJsonLinksPlugin";
        private const string PLUGIN_VERSION = "0.2.4";

        private static readonly string IniFilePath = Path.GetFullPath($"plugins/{PLUGIN_NAME}/{AppConstants.INI_CONFIG_NAME}");
        private static IniConfig _iniConfig = null;
        private static Settings _settings = null;

        private static bool _isPluginInited = false;
        internal static bool IsPluginDisabled = false;

        private static readonly SearchEngine SearchEngine = new SearchEngine();
        private static readonly NavigationHandler NavigationHandler = new NavigationHandler(JumpToLocation);
        private static LinksHighlighter _linksHighlighter = null;

        private static readonly Func<string, IScintillaGateway, ISearchContext> SearchContextFactory = (clickedWord, gateway) => new JsonSearchContext(
            clickedWord,
            gateway,
            gateway.GetCurrentLine(),
            gateway.GetCurrentPos().Value - gateway.LineToPosition(gateway.GetCurrentLine()) - clickedWord.Length
        );


        private static SettingsForm _settingsForm = null;
        private static int _idMyDlg = 1;
        private static readonly Bitmap TbBmp = Properties.Resources.star;
        private static readonly Bitmap TbBmpTbTab = Properties.Resources.star_bmp;
        private static Icon _tbIcon = null;

        internal static void DisablePlugin()
        {
            Logger.Error($"Plugin \"{PLUGIN_NAME}\" will be disabled", null, true);

            IsPluginDisabled = true;
            Logger.SetMode(Logger.Mode.DISABLED, null);
            UserInputHandler.Disable();
            NavigationHandler.Disable();
        }

        private static void ReloadPlugin()
        {
            Logger.Info("Try reload plugin...");

            _isPluginInited = false;
            IsPluginDisabled = false;

            if (!ReloadIniConfig() || !ProcessInit())
            {
                DisablePlugin();
            }
            else
            {
                Logger.Error("Plugin reload: success!", null, true);
            }
        }

        private static bool ReloadIniConfig()
        {
            try
            {
                _iniConfig = IniConfigParser.Parse(IniFilePath);
                Logger.SetMode(_iniConfig.LoggerMode, _iniConfig.LogsDir);

                return true;
            }
            catch (Exception e)
            {
                Logger.SetMode(AppConstants.DEFAULT_LOGGER_MODE, null);
                Logger.Error(e.Message, e, true);

                return false;
            }
        }

        private static bool ProcessInit()
        {
            try
            {
                var gateway = PluginBase.GetGatewayFactory().Invoke();

                // загружаем настройки плагина
                _settings = SettingsParser.Load(_iniConfig);
                Logger.Info($"settings reloaded: mappingFilePathPrefix={_settings.MappingDefaultFilePath}");

                // чтобы SCN_MODIFIED вызывался только, если был добавлен или удален текст
                gateway.SetModEventMask((int) SciMsg.SC_MOD_INSERTTEXT | (int) SciMsg.SC_MOD_DELETETEXT);

                // NPPN_READY вызывается перед последним вызовом NPPN_BUFFERACTIVATED, поэтому нужно инициализировать SearchEngine
                SearchEngine.Reload(_settings, gateway.GetFullCurrentPath());

                // инициализация обработчика кликов мышкой
                UserInputHandler.Reload(HandleMouseEvent, OnKeyboardDown);

                // инициализация поддержки кнопок navigate forward/backward
                NavigationHandler.Reload(gateway.GetCurrentLocation());

                // инициализация поддержки подсветки ссылок
                _linksHighlighter = new LinksHighlighter(gateway, _settings);

                // при запуске NPP вызывается миллиард событий, в том числе и интересующие нас NPPN_BUFFERACTIVATED, SCN_MODIFIED, etc. Но их не нужно обрабатывать до инициализации. 
                _isPluginInited = true;
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e, true);
                return false;
            }
        }

        internal static void CommandMenuInit()
        {
            Logger.Info("COMMAND_MENU_INIT");
            if (!ReloadIniConfig())
            {
                DisablePlugin();
            }

            PluginBase.SetCommand(1, "Settings", ShowSettingsForm, new ShortcutKey(true, false, false, Keys.F1));
            PluginBase.SetCommand(2, "Reload plugin", ReloadPlugin, new ShortcutKey(true, false, false, Keys.F5)); //TODO move to settingsForm
            // PluginBase.SetCommand(2, "", null);

            PluginBase.SetCommand(3, "GoToDefinition", GoToDefinitionCmd, new ShortcutKey(true, true, false, Keys.Enter));
            PluginBase.SetCommand(4, "Navigate Backward", NavigationHandler.NavigateBackward, new ShortcutKey(true, true, false, Keys.Left));
            PluginBase.SetCommand(5, "Navigate Forward", NavigationHandler.NavigateForward, new ShortcutKey(true, true, false, Keys.Right));
            PluginBase.SetCommand(6, "", null);

            PluginBase.SetCommand(7, "About", () => MessageBox.Show($@"Plugin: {PLUGIN_NAME}_v{PLUGIN_VERSION}"), new ShortcutKey(false, false, false, Keys.None));
        }

        public static void OnNotification(ScNotification notification)
        {
            if (IsPluginDisabled) return;

            var notificationType = notification.Header.Code;

            if (notificationType == (uint) NppMsg.NPPN_READY)
            {
                Logger.Info("NPPN_READY");

                if (!ProcessInit())
                {
                    DisablePlugin();
                }

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

                case (uint) SciMsg.SCN_SAVEPOINTREACHED:
                    // пользователь сохранил изменения в текущем файле (ctrl + s)
                    SearchEngine.FireSaveFile();
                    Logger.Info("SCN_SAVEPOINTREACHED");
                    break;

                case (uint) SciMsg.SCN_UPDATEUI:
                {
                    // SCROLL/INPUT/COLLAPSE and other events
                    _linksHighlighter.UpdateUi();
                    break;
                }
            }
        }

        private static void OnKeyboardDown(int keyCode)
        {
            var gateway = PluginBase.GetGatewayFactory().Invoke();
            var currentLine = gateway.GetCurrentLine();
            NavigationHandler.UpdateHistory(new JumpLocation(gateway.GetFullCurrentPath(), currentLine), NavigateActionType.KEYBOARD_DOWN);
        }

        private static void HandleMouseEvent(UserInputHandler.MouseMessage msg)
        {
            switch (msg)
            {
                case UserInputHandler.MouseMessage.WM_LBUTTONUP:
                    if (Control.ModifierKeys == Keys.Control)
                    {
                        var success = GoToDefinition();
                        if (success) return;
                    }

                    break;

                case UserInputHandler.MouseMessage.WM_RBUTTONUP:
                    break;

                default:
                    return;
            }

            var gateway = PluginBase.GetGatewayFactory().Invoke();
            NavigationHandler.UpdateHistory(gateway.GetCurrentLocation(), NavigateActionType.MOUSE_CLICK);
        }

        private static void GoToDefinitionCmd()
        {
            if (IsPluginDisabled) return;
            GoToDefinition();
        }

        private static bool GoToDefinition()
        {
            var gateway = PluginBase.GetGatewayFactory().Invoke();
            string selectedWord = gateway.GetCurrentWord();

            if (!string.IsNullOrEmpty(selectedWord))
            {
                JumpLocation jumpLocation = SearchEngine.FindDefinitionLocation(SearchContextFactory.Invoke(selectedWord, gateway));

                if (jumpLocation != null)
                {
                    JumpToLocation(jumpLocation);
                    NavigationHandler.UpdateHistory(jumpLocation, NavigateActionType.MOUSE_CLICK);
                    return true;
                }
            }

            gateway.GrabFocus();

            if (_settings.SoundEnabled)
            {
                System.Media.SystemSounds.Asterisk.Play();
            }

            return false;
        }

        private static void JumpToLocation(JumpLocation jumpLocation)
        {
            string file = jumpLocation.FilePath;
            int line = jumpLocation.Line;

            Logger.Info($"Opening file '{file}'");

            var gateway = PluginBase.GetGatewayFactory().Invoke();
            gateway.OpenFile(file);

            // задержка фиксит багу с выделением текста при переходе
            ThreadUtils.ExecuteDelayed(() => gateway.JumpToLine(line), _settings.JumpToLineDelay);
        }

        internal static void OnShutdown()
        {
            UserInputHandler.Disable();
        }

        #region " Layout Base "

        private static void ShowSettingsForm()
        {
            if (_settingsForm == null)
            {
                _settingsForm = new SettingsForm();

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

                // NppTbData _nppTbData = new NppTbData();
                // _nppTbData.hClient = _settingsForm.Handle;
                // _nppTbData.pszName = "My dockable dialog";
                // _nppTbData.dlgID = _idMyDlg;
                // _nppTbData.uMask = NppTbMsg.DWS_DF_CONT_RIGHT | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR;
                // _nppTbData.hIconTab = (uint) _tbIcon.Handle;
                // _nppTbData.pszModuleName = PLUGIN_NAME;
                // IntPtr _ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(_nppTbData));
                // Marshal.StructureToPtr(_nppTbData, _ptrNppTbData, false);

                // Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) NppMsg.NPPM_DMMREGASDCKDLG, 0, _ptrNppTbData);
                // Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) NppMsg.NPPM_DMMSHOW, 0, _settingsForm.Handle);
            }


            if (_settingsForm.ShowDialog(_iniConfig.Clone(), _settings) == DialogResult.OK)
            {
                ReloadPlugin();
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