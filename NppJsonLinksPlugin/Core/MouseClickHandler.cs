using System;
using NppJsonLinksPlugin.PluginInfrastructure;

namespace NppJsonLinksPlugin.Core
{
    public static class MouseClickHandler
    {
        public static MouseEvent OnMouseClick = null;

        private static IntPtr _oldMainWndProc = IntPtr.Zero;
        private static readonly Win32.WindowProc NewMainWndProc = MainWndProc;
        private static IntPtr _oldSecondWndProc = IntPtr.Zero;
        private static readonly Win32.WindowProc NewSecondWndProc = SecondWndProc;

        public delegate void MouseEvent(MouseMessage msg);

        public enum MouseMessage
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
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
            if (OnMouseClick == null || !Enum.IsDefined(typeof(MouseMessage), msg))
                return Win32.CallWindowProcW(oldWndProc, hWnd, msg, wParam, lParam);

            int res = Win32.CallWindowProcW(oldWndProc, hWnd, msg, wParam, lParam);
            OnMouseClick.Invoke((MouseMessage) msg);
            return res;
        }
    }
}