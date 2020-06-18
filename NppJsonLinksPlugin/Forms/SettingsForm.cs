using System;
using System.Windows.Forms;
using NppJsonLinksPlugin.Configuration;

namespace NppJsonLinksPlugin.Forms
{
    public partial class SettingsForm : Form
    {
        private Settings _settings;
        private IniConfig _config;

        public SettingsForm()
        {
            InitializeComponent();
        }

        public DialogResult ShowDialog(IniConfig config, Settings settings)
        {
            _settings = settings;
            _config = config;
           return base.ShowDialog();
        }
        
        private TextboxWrapper settingsJsonUriTextboxWrapper;
        private TextboxWrapper defaultMappingPathTextboxWrapper;
        private TextboxWrapper highlightingLinesLimitTextboxWrapper;
        private TextboxWrapper loggerPathTextboxWrapper;

        private TextboxWrapper addPlaceholder(TextBox textBox, string placehoder)
        {
            return new TextboxWrapper(textBox, placehoder);
        }
        
        private void SettingsForm_Load(object sender, EventArgs e)
        {
            settingsJsonUriTextboxWrapper.SetInitialText(_config.SettingsJsonUri);

            loggerPathTextboxWrapper.SetInitialText(_config.LogsDir);
            loggerModeComboBox.SelectedIndex = (int) _config.LoggerMode;

            highlightingLinesLimitTextboxWrapper.SetInitialText(_settings.ProcessingHighlightedLinesLimit.ToString());
            highlightingEnabledComboBox.SelectedIndex = Convert.ToInt32(_settings.HighlightingEnabled);
            soundEnabledComboBox.SelectedIndex = Convert.ToInt32(_settings.SoundEnabled);
            defaultMappingPathTextboxWrapper.SetInitialText(_settings.MappingDefaultFilePath);
        }
    }
}