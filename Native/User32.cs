using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Thumbs
{   
    class User32
    {
        public const int GWL_STYLE = -16;

        public const ulong WS_VISIBLE = 0x10000000L,
            WS_BORDER = 0x00800000L,
            TARGETWINDOW = WS_BORDER | WS_VISIBLE;

        const string DllName = "user32";

        [DllImport(DllName)]
        public static extern ulong GetWindowLongA(IntPtr hWnd, int nIndex);

        [DllImport(DllName)]
        public static extern int EnumWindows(EnumWindowsCallback lpEnumFunc, int lParam);
        
        [DllImport(DllName)]
        public static extern void GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
    }
}
