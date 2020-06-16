using System;
using NppJsonLinksPlugin.PluginInfrastructure;

namespace NppJsonLinksPlugin.Core
{
    /**
     * Работает только для 32-битной версии
     */
    public static class UserInputHandler
    {
        private static MouseEvent _onMouseAction = null;
        private static KeyboardEvent _onKeyboardDown = null;

        private static IntPtr _oldMainWndProc = IntPtr.Zero;
        private static readonly Win32.WindowProc NewMainWndProc = MainWndProc;
        private static IntPtr _oldSecondWndProc = IntPtr.Zero;
        private static readonly Win32.WindowProc NewSecondWndProc = SecondWndProc;

        public delegate void MouseEvent(MouseMessage msg);

        public delegate void KeyboardEvent(int keyCode);

        public enum MouseMessage
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }

        private const int WM_KEYDOWN = 0x0100;

        public enum KeyCode
        {
            VK_END = 0x23,
            VK_HOME = 0x24,
            VK_LEFT = 0x25,
            VK_UP = 0x26,
            VK_RIGHT = 0x27,
            VK_DOWN = 0x28
        }

        internal static void Reload(MouseEvent onMouseAction, KeyboardEvent onKeyboardDown)
        {
            Disable();
            _onMouseAction = onMouseAction;
            _onKeyboardDown = onKeyboardDown;
            Enable();
        }

        internal static void Enable()
        {
            if (_oldMainWndProc == IntPtr.Zero)
            {
                _oldMainWndProc = Win32.SetWindowLongW(PluginBase.nppData._scintillaMainHandle, Win32.GWL_WNDPROC, NewMainWndProc);
            }

            if (_oldSecondWndProc == IntPtr.Zero)
            {
                _oldSecondWndProc = Win32.SetWindowLongW(PluginBase.nppData._scintillaSecondHandle, Win32.GWL_WNDPROC, NewSecondWndProc);
            }
        }

        internal static void Disable()
        {
            if (_oldMainWndProc != IntPtr.Zero)
            {
                Win32.SetWindowLongW(PluginBase.nppData._scintillaMainHandle, Win32.GWL_WNDPROC, _oldMainWndProc);
                _oldMainWndProc = IntPtr.Zero;
            }

            if (_oldSecondWndProc != IntPtr.Zero)
            {
                Win32.SetWindowLongW(PluginBase.nppData._scintillaSecondHandle, Win32.GWL_WNDPROC, _oldSecondWndProc);
                _oldSecondWndProc = IntPtr.Zero;
            }
        }

        private static int MainWndProc(IntPtr hWnd, int msg, int wParam, int lParam)
        {
            return CommonWndProc(_oldMainWndProc, hWnd, msg, wParam, lParam);
        }

        private static int SecondWndProc(IntPtr hWnd, int msg, int wParam, int lParam)
        {
            return CommonWndProc(_oldSecondWndProc, hWnd, msg, wParam, lParam);
        }

        private static int CommonWndProc(IntPtr oldWndProc, IntPtr hWnd, int msg, int wParam, int lParam)
        {
            if (Main.IsPluginDisabled) return 0;

            try
            {
                var result = Win32.CallWindowProcW(oldWndProc, hWnd, msg, wParam, lParam);

                if (msg == WM_KEYDOWN && _onKeyboardDown != null)
                {
                    _onKeyboardDown.Invoke(lParam);
                }
                else if (_onMouseAction != null && Enum.IsDefined(typeof(MouseMessage), msg))
                {
                    _onMouseAction.Invoke((MouseMessage) msg);
                }

                return result;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
                return 0;
            }
        }
    }
}