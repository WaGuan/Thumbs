using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Thumbs
{
    delegate bool EnumWindowsCallback(IntPtr hwnd, int lParam);

    class DWMApi
    {
        public const int DWM_TNP_VISIBLE = 0x8,
            DWM_TNP_OPACITY = 0x4,
            DWM_TNP_RECTDESTINATION = 0x1;

        [DllImport("dwmapi.dll")]
        public static extern int DwmRegisterThumbnail(IntPtr dest, IntPtr src, out IntPtr thumb);

        [DllImport("dwmapi.dll")]
        public static extern int DwmUnregisterThumbnail(IntPtr thumb);

        [DllImport("dwmapi.dll")]
        public static extern int DwmQueryThumbnailSourceSize(IntPtr thumb, out PSIZE size);

        [DllImport("dwmapi.dll")]
        public static extern int DwmUpdateThumbnailProperties(IntPtr hThumb, ref DWM_THUMBNAIL_PROPERTIES props);
    }

    class User32
    {
        public const int GWL_STYLE = -16;

        public const ulong WS_VISIBLE = 0x10000000L,
            WS_BORDER = 0x00800000L,
            TARGETWINDOW = WS_BORDER | WS_VISIBLE;
        
        [DllImport("user32.dll")]
        public static extern ulong GetWindowLongA(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int EnumWindows(EnumWindowsCallback lpEnumFunc, int lParam);
        
        [DllImport("user32.dll")]
        public static extern void GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
    }
}
