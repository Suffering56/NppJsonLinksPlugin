using System;
using System.Windows.Forms;
using NppJsonLinksPlugin.Configuration;
using NppJsonLinksPlugin.Core;

namespace NppJsonLinksPlugin.Forms
{
    public partial class SettingsForm : Form
    {
        private Settings _settings;
        private IniConfig _config;

        private readonly TextboxWrapper _settingsJsonUriTextboxWrapper;
        private readonly TextboxWrapper _defaultMappingPathTextboxWrapper;
        private readonly TextboxWrapper _highlightingLinesLimitTextboxWrapper;
        private readonly TextboxWrapper _loggerPathTextboxWrapper;
        private readonly TextboxWrapper _jumpToLineDelayTextboxWrapper;

        public SettingsForm()
        {
            InitializeComponent();

            _settingsJsonUriTextboxWrapper = WrapWithPlaceholder(settingsJsonUriTextbox, AppConstants.DEFAULT_SETTINGS_URI);
            _defaultMappingPathTextboxWrapper = WrapWithPlaceholder(defaultMappingPathTextbox, AppConstants.MAPPING_PATH_PLACEHOLDER);
            _highlightingLinesLimitTextboxWrapper = WrapWithPlaceholder(highlightingLinesLimitTextbox, AppConstants.DEFAULT_PROCESSING_HIGHLIGHTING_LINES_LIMIT.ToString());
            _loggerPathTextboxWrapper = WrapWithPlaceholder(loggerPathTextbox, AppConstants.DEFAULT_LOGGER_PATH);
            _jumpToLineDelayTextboxWrapper = WrapWithPlaceholder(jumpToLineDelayTextbox, AppConstants.DEFAULT_JUMP_TO_LINE_DELAY.ToString());
        }

        public DialogResult ShowDialog(IniConfig config, Settings settings)
        {
            _settings = settings;
            _config = config;
            return base.ShowDialog();
        }

        private TextboxWrapper WrapWithPlaceholder(TextBox textBox, string placehoder)
        {
            return new TextboxWrapper(textBox, placehoder);
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            _settingsJsonUriTextboxWrapper.SetInitialText(_config.SettingsJsonUri);

            _loggerPathTextboxWrapper.SetInitialText(_config.LogsDir);
            loggerModeComboBox.SelectedIndex = (int) _config.LoggerMode;

            _highlightingLinesLimitTextboxWrapper.SetInitialText(_settings.ProcessingHighlightedLinesLimit.ToString());
            highlightingEnabledComboBox.SelectedIndex = Convert.ToInt32(_settings.HighlightingEnabled);
            soundEnabledComboBox.SelectedIndex = Convert.ToInt32(_settings.SoundEnabled);
            _defaultMappingPathTextboxWrapper.SetInitialText(_settings.MappingDefaultFilePath);
            _jumpToLineDelayTextboxWrapper.SetInitialText(_settings.JumpToLineDelay.ToString());
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Logger.Info("settings form closed: Cancel");
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            Logger.Info("settings form closed: Save");

            _config.SettingsJsonUri = _settingsJsonUriTextboxWrapper.GetText();
            _config.MappingDefaultFilePath = _defaultMappingPathTextboxWrapper.GetText();
            _config.LogsDir = _loggerPathTextboxWrapper.GetText();

            var loggerMode = ConvertUtils.ToLoggerMode(loggerModeComboBox.Text, () => $"cannot parse loggerMode: {loggerModeComboBox.Text}");
            if (!loggerMode.HasValue) return;
            _config.LoggerMode = loggerMode.Value;

            var booleanValue = ConvertUtils.ToBool(highlightingEnabledComboBox.Text);
            if (!booleanValue.HasValue) return;
            _config.HighlightingEnabled = booleanValue.Value;

            booleanValue = ConvertUtils.ToBool(soundEnabledComboBox.Text);
            if (!booleanValue.HasValue) return;
            _config.SoundEnabled = booleanValue.Value;

            var intValue = _highlightingLinesLimitTextboxWrapper.GetInt();
            if (!intValue.HasValue) return;
            _config.ProcessingHighlightedLinesLimit = intValue.Value;

            intValue = _jumpToLineDelayTextboxWrapper.GetInt();
            if (!intValue.HasValue) return;
            _config.JumpToLineDelay = intValue.Value;
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            var settingsUri = _settingsJsonUriTextboxWrapper.GetText();
            RawSettings rawSettings;
            try
            {
                rawSettings = SettingsParser.Parse(settingsUri);
            }
            catch (Exception)
            {
                Logger.Error($"cannot load default settings by invalid settings uri: \"{settingsUri}\". try reload by default settings uri path.", null, true);
                settingsUri = AppConstants.DEFAULT_SETTINGS_URI;
                _settingsJsonUriTextboxWrapper.SetInitialText(settingsUri);
                rawSettings = SettingsParser.Parse(settingsUri);
            }

            _loggerPathTextboxWrapper.SetInitialText(AppConstants.DEFAULT_LOGGER_PATH);
            loggerModeComboBox.SelectedIndex = (int) AppConstants.DEFAULT_LOGGER_MODE;

            _defaultMappingPathTextboxWrapper.SetInitialText("");
            highlightingEnabledComboBox.SelectedIndex = Convert.ToInt32(rawSettings.HighlightingEnabled);
            _highlightingLinesLimitTextboxWrapper.SetInitialText(rawSettings.ProcessingHighlightedLinesLimit.ToString());
            soundEnabledComboBox.SelectedIndex = Convert.ToInt32(rawSettings.SoundEnabled);
            _jumpToLineDelayTextboxWrapper.SetInitialText(rawSettings.JumpToLineDelay.ToString());
        }
    }
}