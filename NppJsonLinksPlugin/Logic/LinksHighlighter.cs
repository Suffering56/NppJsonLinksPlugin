using System;
using System.Collections.Generic;
using System.Linq;
using NppJsonLinksPlugin.Configuration;
using NppJsonLinksPlugin.Core;
using NppJsonLinksPlugin.Logic.Parser;
using NppJsonLinksPlugin.Logic.Parser.Json;
using NppJsonLinksPlugin.PluginInfrastructure.Gateway;

namespace NppJsonLinksPlugin.Logic
{
    public class LinksHighlighter
    {
        private const int HIGHLIGHT_INDICATOR_ID = 32;

        private const int STYLE_NONE = 5;
        private const int STYLE_UNDERLINE = 0;

        private readonly IScintillaGateway _gateway;
        private readonly Settings _settings;

        public LinksHighlighter(IScintillaGateway gateway, Settings settings)
        {
            _gateway = gateway;
            _settings = settings;

            gateway.SetIndicatorStyle(HIGHLIGHT_INDICATOR_ID, STYLE_UNDERLINE);
        }

        private void Highlight(Word word, string value, int lineIndex, int linePosition)
        {
            try
            {
                var position = _gateway.LineToPosition(lineIndex) + linePosition;
                // var length = word.GetWordString().Length;
                // var length = value.Length;
                // var length = 1;
                _gateway.ApplyIndicatorStyleForRange(HIGHLIGHT_INDICATOR_ID, position, 0);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        // public void Highlight()
        // {
        //     _gateway.ApplyIndicatorStyleForRange(HIGHLIGHT_INDICATOR_ID, _gateway.GetCurrentPos().Value, 5);
        //     counter++;
        //
        //     if (counter % 3 != 0) return;
        //     CleanCurrentFileHighlighting();
        // }

        // private void CleanCurrentFileHighlighting()
        // {
        //     _gateway.ClearIndicatorStyleForRange(HIGHLIGHT_INDICATOR_ID, 0, _gateway.GetTextLength());
        // }
    }
}