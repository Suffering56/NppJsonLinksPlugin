using System.Collections.Generic;
using NppJsonLinksPlugin.Logic;

namespace NppJsonLinksPlugin.Core
{
    public class NavigationHandler
    {
        public delegate void JumpHandler(JumpLocation jumpLocation);

        private readonly JumpHandler _jumpHandler;

        private int _historyPosition = 0;
        private JumpLocation _prevLocation = null;
        private readonly List<JumpLocation> _navigationHistory = new List<JumpLocation>();

        public NavigationHandler(JumpHandler jumpHandler)
        {
            _jumpHandler = jumpHandler;
        }

        public void Disable()
        {
            _prevLocation = null;
        }

        public void Reload(JumpLocation currentLocation)
        {
            _historyPosition = 0;
            _navigationHistory.Clear();
            _prevLocation = currentLocation;
        }

        public void UpdateHistory(JumpLocation newLocation, NavigateActionType actionType)
        {
            if (IsNavigationDisabled()) return;

            var historySize = _navigationHistory.Count;
            if (_historyPosition > historySize)
            {
                Logger.Error($"invalid navigation history position={_historyPosition}, because history size={historySize}");
                Reload(newLocation);
                return;
            }

            if (newLocation.Line == _prevLocation.Line) return;

            switch (actionType)
            {
                case NavigateActionType.GO_FORWARD:
                    if (_historyPosition == historySize) return;
                    _historyPosition++;
                    _prevLocation = newLocation;
                    break;

                case NavigateActionType.GO_BACKWARD:
                    if (_historyPosition == 0) return;

                    if (_historyPosition == historySize)
                    {
                        _navigationHistory.Add(_prevLocation);
                    }

                    _historyPosition--;
                    _prevLocation = newLocation;
                    break;

                case NavigateActionType.MOUSE_CLICK:
                    if (_historyPosition == historySize)
                    {
                        _historyPosition++;
                        _navigationHistory.Add(_prevLocation);
                        _prevLocation = newLocation;

                        break;
                    }

                    _historyPosition++;
                    _prevLocation = newLocation;

                    // мы находимся не в самом конце истории. поэтому при изменении - вся остальная часть истории забывается
                    if (_historyPosition < historySize) _navigationHistory.RemoveRange(_historyPosition, historySize - _historyPosition);
                    break;

                case NavigateActionType.KEYBOARD_DOWN:
                    _prevLocation = newLocation;
                    break;
            }
        }

        public void NavigateBackward()
        {
            if (IsNavigationDisabled()) return;

            if (_historyPosition > 0 && _historyPosition <= _navigationHistory.Count)
            {
                var jumpLocation = _navigationHistory[_historyPosition - 1];
                _jumpHandler(jumpLocation);
                UpdateHistory(jumpLocation, NavigateActionType.GO_BACKWARD);
            }
        }

        public void NavigateForward()
        {
            if (IsNavigationDisabled()) return;

            if (_historyPosition + 1 < _navigationHistory.Count)
            {
                var jumpLocation = _navigationHistory[_historyPosition + 1];
                _jumpHandler(jumpLocation);
                UpdateHistory(jumpLocation, NavigateActionType.GO_FORWARD);
            }
        }

        private bool IsNavigationDisabled()
        {
            return _prevLocation == null;
        }
    }

    public enum NavigateActionType
    {
        GO_FORWARD,
        GO_BACKWARD,
        MOUSE_CLICK,
        KEYBOARD_DOWN
    }
}