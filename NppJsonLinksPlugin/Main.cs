using System;
using System.Diagnostics.CodeAnalysis;
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
        private const string PLUGIN_VERSION = "0.4.2";

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

        internal static void DisablePlugin(bool showErrorBox = true)
        {
            if (showErrorBox)
            {
                Logger.Error($"Plugin \"{PLUGIN_NAME}\" will be disabled", null, true);
            }

            IsPluginDisabled = true;
            Logger.SetMode(Logger.Mode.DISABLED);
            _linksHighlighter?.Dispose();
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
                Logger.InfoBox("Plugin reload: success!");
            }
        }

        private static bool ReloadIniConfig()
        {
            try
            {
                _iniConfig = IniConfigParser.Parse();
                Logger.SetMode(_iniConfig.LoggerMode);

                return true;
            }
            catch (Exception e)
            {
                Logger.SetMode(Logger.Mode.DISABLED_WITH_ALERTS);
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
                _settings = SettingsParser.LoadMapping(_iniConfig);
                Logger.Info($"settings reloaded: WorkingDirectory={_settings.Config.WorkingDirectory}");

                // чтобы SCN_MODIFIED вызывался только, если был добавлен или удален текст
                gateway.SetModEventMask((int) SciMsg.SC_MOD_INSERTTEXT | (int) SciMsg.SC_MOD_DELETETEXT);

                // NPPN_READY вызывается перед последним вызовом NPPN_BUFFERACTIVATED, поэтому нужно инициализировать SearchEngine
                SearchEngine.Reload(_settings, gateway.GetFullCurrentPath());

                // инициализация обработчика кликов мышкой
                UserInputHandler.Reload(HandleMouseEvent, OnKeyboardDown);

                // инициализация поддержки кнопок navigate forward/backward
                NavigationHandler.Reload(gateway.GetCurrentLocation());

                // инициализация поддержки подсветки ссылок
                _linksHighlighter?.Dispose();
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

            PluginBase.SetCommand(1, "GoToDefinition", GoToDefinitionCmd, new ShortcutKey(true, true, false, Keys.Enter));
            PluginBase.SetCommand(2, "Navigate Backward", NavigationHandler.NavigateBackward, new ShortcutKey(true, true, false, Keys.Left));
            PluginBase.SetCommand(3, "Navigate Forward", NavigationHandler.NavigateForward, new ShortcutKey(true, true, false, Keys.Right));
            PluginBase.SetCommand(4, "", null);

            PluginBase.SetCommand(5, "Disable plugin", DisablePluginCmd, new ShortcutKey());
            PluginBase.SetCommand(6, "Reload/enable plugin", ReloadPluginCmd, new ShortcutKey());
            PluginBase.SetCommand(7, "", null);
            PluginBase.SetCommand(8, "Settings", ShowSettingsForm, new ShortcutKey());
            PluginBase.SetCommand(9, "", null);
            PluginBase.SetCommand(10, "About", () => MessageBox.Show($@"Plugin: {PLUGIN_NAME}_v{PLUGIN_VERSION}"), new ShortcutKey());
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
                    break;

                case (uint) SciMsg.SCN_SAVEPOINTREACHED:
                    // пользователь сохранил изменения в текущем файле (ctrl + s)
                    SearchEngine.FireSaveFile();
                    break;

                case (uint) SciMsg.SCN_UPDATEUI:
                {
                    // SCROLL/INPUT/COLLAPSE and other events
                    _linksHighlighter.MarkUpdated();
                    break;
                }
            }
        }

        private static void OnKeyboardDown(int keyCode)
        {
            // игнорируем нажатие кнопок, которые нас не перемещают в пространстве
            // мы не можем выбрать только те кнопки, которые нас могут перемещать (UP, DOWN... CTRL + X/V/Z/Y, ENTER и тд),
            //     потому что у игрока может быть выделенный фрагмент текста и любая кнопка его удалит
            switch ((UserInputHandler.KeyCode) keyCode)
            {
                case UserInputHandler.KeyCode.VK_CONTROL:
                case UserInputHandler.KeyCode.VK_LSHIFT:
                case UserInputHandler.KeyCode.VK_RSHIFT:
                case UserInputHandler.KeyCode.VK_MENU:
                case UserInputHandler.KeyCode.VK_PAUSE:
                case UserInputHandler.KeyCode.VK_CAPITAL:
                case UserInputHandler.KeyCode.VK_SNAPSHOT:
                case UserInputHandler.KeyCode.VK_ESCAPE:
                case UserInputHandler.KeyCode.VK_F1:
                case UserInputHandler.KeyCode.VK_F2:
                case UserInputHandler.KeyCode.VK_F3:
                case UserInputHandler.KeyCode.VK_F4:
                case UserInputHandler.KeyCode.VK_F5:
                case UserInputHandler.KeyCode.VK_F6:
                case UserInputHandler.KeyCode.VK_F7:
                case UserInputHandler.KeyCode.VK_F8:
                case UserInputHandler.KeyCode.VK_F9:
                case UserInputHandler.KeyCode.VK_F10:
                case UserInputHandler.KeyCode.VK_F11:
                case UserInputHandler.KeyCode.VK_F12:
                    return;
            }

            var gateway = PluginBase.GetGatewayFactory().Invoke();
            var currentLine = gateway.GetCurrentLine();

            NavigationHandler.UpdateHistory(new JumpLocation(gateway.GetFullCurrentPath(), currentLine), NavigateActionType.KEYBOARD_DOWN);
        }

        private static void HandleMouseEvent(UserInputHandler.MouseMessage msg)
        {
            bool goToDefinition = false;

            switch (msg)
            {
                case UserInputHandler.MouseMessage.WM_LBUTTONUP:
                    if (Control.ModifierKeys == Keys.Control)
                    {
                        goToDefinition = true;
                    }

                    break;

                case UserInputHandler.MouseMessage.WM_RBUTTONUP:
                    break;
                case UserInputHandler.MouseMessage.WM_MOUSEWHEEL:
                    //TODO: почему-то клик по колесику мыши вообще не инициирует никакой mouse event
                    // goToDefinition = true;
                    break;

                default:
                    return;
            }

            NavigationHandler.UpdateHistory(
                PluginBase.GetGatewayFactory().Invoke().GetCurrentLocation(),
                NavigateActionType.MOUSE_CLICK
            );

            if (goToDefinition)
            {
                GoToDefinition();
            }
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

            if (_settings.Config.SoundEnabled)
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

            if (_settings.Config.JumpToLineDelay > 0)
            {
                // задержка фиксит багу с выделением текста при переходе
                ThreadUtils.ExecuteDelayed(() => gateway.JumpToLine(line), _settings.Config.JumpToLineDelay);
            }
            else
            {
                gateway.JumpToLine(line);
            }
        }

        internal static void OnShutdown()
        {
            UserInputHandler.Disable();
        }


        private static void ReloadPluginCmd()
        {
            var result = MessageBox.Show($@"Do you really want to reload plugin: {PLUGIN_NAME}?", @"Reload plugin?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                ReloadPlugin();
            }
        }

        private static void DisablePluginCmd()
        {
            var result = MessageBox.Show($@"Do you really want to disable plugin: {PLUGIN_NAME}?", @"Disable plugin?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                DisablePlugin(false);
            }
        }

        private static void ShowSettingsForm()
        {
            _settingsForm ??= new SettingsForm();
            var modifiedConfig = _iniConfig.Clone();

            if (_settingsForm.ShowDialog(modifiedConfig) == DialogResult.OK)
            {
                if (modifiedConfig.Save())
                {
                    ReloadPlugin();
                }
            }
        }
    }
}