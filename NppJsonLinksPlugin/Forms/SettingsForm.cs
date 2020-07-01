using System;
using System.Windows.Forms;
using NppJsonLinksPlugin.Configuration;
using NppJsonLinksPlugin.Core;
using static NppJsonLinksPlugin.AppConstants;

namespace NppJsonLinksPlugin.Forms
{
    public partial class SettingsForm : Form
    {
        private IniConfig _mutableConfig;

        private readonly TextboxWrapper _mappingRemoteUrlTextBoxWrapper;
        private readonly TextboxWrapper _workingDirectoryTextBoxWrapper;
        private readonly TextboxWrapper _jumpToLineDelayTextBoxWrapper;
        private readonly TextboxWrapper _mappingDefaultSrcOrderTextBoxWrapper;
        private readonly TextboxWrapper _highlightingTimerIntervalTextBoxWrapper;

        public SettingsForm()
        {
            InitializeComponent();

            _mappingRemoteUrlTextBoxWrapper = WrapWithPlaceholder(mappingRemoteUrlTextbox, Placeholders.MAPPING_REMOTE_URL);
            _workingDirectoryTextBoxWrapper = WrapWithPlaceholder(workingDirectoryTextbox, Placeholders.WORKING_DIRECTORY);
            _mappingDefaultSrcOrderTextBoxWrapper = WrapWithPlaceholder(mappingDefailtSrcOrderTextbox, Placeholders.MAPPING_DEFAULT_SRC_ORDER);
            _highlightingTimerIntervalTextBoxWrapper = WrapWithPlaceholder(highlightingTimerIntervalTextbox, Placeholders.HIGHLIGHTING_TIMER_INTERVAL);
            _jumpToLineDelayTextBoxWrapper = WrapWithPlaceholder(jumpToLineDelayTextbox, Placeholders.JUMP_TO_LINE_DELAY);
        }

        public DialogResult ShowDialog(IniConfig mutableConfig)
        {
            _mutableConfig = mutableConfig;
            return base.ShowDialog();
        }

        private static TextboxWrapper WrapWithPlaceholder(TextBox textBox, string placeHolder)
        {
            return new TextboxWrapper(textBox, placeHolder);
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            //1
            _mappingRemoteUrlTextBoxWrapper.SetInitialText(_mutableConfig.MappingRemoteUrl);
            //2
            loggerModeComboBox.SelectedIndex = (int) _mutableConfig.LoggerMode;
            //3
            _workingDirectoryTextBoxWrapper.SetInitialText(_mutableConfig.WorkingDirectory);
            //4
            _mappingDefaultSrcOrderTextBoxWrapper.SetInitialText(_mutableConfig.MappingDefaultSrcOrder.ToString());
            //5
            highlightingEnabledComboBox.SelectedIndex = Convert.ToInt32(_mutableConfig.HighlightingEnabled);
            //6
            _highlightingTimerIntervalTextBoxWrapper.SetInitialText(_mutableConfig.HighlightingTimerInterval.ToString());
            //7
            _jumpToLineDelayTextBoxWrapper.SetInitialText(_mutableConfig.JumpToLineDelay.ToString());
            //8
            soundEnabledComboBox.SelectedIndex = Convert.ToInt32(_mutableConfig.SoundEnabled);
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Logger.Info("settings form closed: Cancel");
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            Logger.Info("settings form closed: Save");

            int? intValue;
            bool? booleanValue;

            //1
            _mutableConfig.MappingRemoteUrl = _mappingRemoteUrlTextBoxWrapper.GetText();

            //2
            var loggerMode = ConvertUtils.ToLoggerMode(loggerModeComboBox.Text, () => $"cannot parse loggerMode: {loggerModeComboBox.Text}");
            if (!loggerMode.HasValue) return;
            _mutableConfig.LoggerMode = loggerMode.Value;

            //3
            _mutableConfig.WorkingDirectory = _workingDirectoryTextBoxWrapper.GetText();

            //4
            intValue = _mappingDefaultSrcOrderTextBoxWrapper.GetInt();
            if (!intValue.HasValue) return;
            _mutableConfig.MappingDefaultSrcOrder = intValue.Value;

            //5
            booleanValue = ConvertUtils.ToBool(highlightingEnabledComboBox.Text);
            if (!booleanValue.HasValue) return;
            _mutableConfig.HighlightingEnabled = booleanValue.Value;

            //6
            intValue = _highlightingTimerIntervalTextBoxWrapper.GetInt();
            if (!intValue.HasValue) return;
            _mutableConfig.HighlightingTimerInterval = intValue.Value;

            //7
            intValue = _jumpToLineDelayTextBoxWrapper.GetInt();
            if (!intValue.HasValue) return;
            _mutableConfig.JumpToLineDelay = intValue.Value;

            //8
            booleanValue = ConvertUtils.ToBool(soundEnabledComboBox.Text);
            if (!booleanValue.HasValue) return;
            _mutableConfig.SoundEnabled = booleanValue.Value;
        }

        private void reloadMappingButton_Click(object sender, EventArgs e)
        {
            var mappingRemoteUrl = _mappingRemoteUrlTextBoxWrapper.GetText();
            Uri uri;

            try
            {
                uri = ConvertUtils.ToUri(mappingRemoteUrl);
            }
            catch (Exception)
            {
                return;
            }

            try
            {
                SettingsParser.DownloadRemoteMapping(uri);
            }
            catch (Exception)
            {
                Logger.Error($"cannot reload mapping by remote URL: \"{uri}\".", null, true);
            }
        }
    }
}