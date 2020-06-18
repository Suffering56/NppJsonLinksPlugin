using System;
using System.Drawing;
using System.Windows.Forms;
using NppJsonLinksPlugin.Core;

namespace NppJsonLinksPlugin.Forms
{
    public class TextboxWrapper
    {
        private TextBox _textBox;
        private string _placehoder;
        private bool _placeholderActive;

        public TextboxWrapper(TextBox textBox, string placehoder)
        {
            _textBox = textBox;
            _placehoder = placehoder;

            TryShowPlaceholder(null, null);

            textBox.GotFocus += TryHidePlaceholder;
            textBox.LostFocus += TryShowPlaceholder;
        }

        public void SetInitialText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            _textBox.Text = text;
            DisablePlaceholder();
        }

        public string GetText()
        {
            if (_placeholderActive)
            {
                return "";
            }

            return _textBox.Text;
        }

        public int? GetInt()
        {
            return ConvertUtils.ToInt(GetText());
        }

        private void TryHidePlaceholder(object sender, EventArgs e)
        {
            if (_placeholderActive)
            {
                _textBox.Text = "";
                DisablePlaceholder();
            }
        }

        private void TryShowPlaceholder(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_textBox.Text))
            {
                _textBox.Text = _placehoder;
                EnablePlaceholder();
            }
        }

        private void EnablePlaceholder()
        {
            _placeholderActive = true;
            _textBox.ForeColor = Color.LightGray;
        }

        private void DisablePlaceholder()
        {
            _placeholderActive = false;
            _textBox.ForeColor = Color.Black;
        }
    }
}