using System;
using System.Runtime.InteropServices;

namespace Thumbs
{
    class DWMApi
    {
        public const int DWM_TNP_VISIBLE = 0x8,
            DWM_TNP_OPACITY = 0x4,
            DWM_TNP_RECTDESTINATION = 0x1;

        const string DllName = "dwmapi";

        [DllImport(DllName)]
        public static extern int DwmRegisterThumbnail(IntPtr dest, IntPtr src, out IntPtr thumb);

        [DllImport(DllName)]
        public static extern int DwmUnregisterThumbnail(IntPtr thumb);

        [DllImport(DllName)]
        public static extern int DwmQueryThumbnailSourceSize(IntPtr thumb, out PSIZE size);

        [DllImport(DllName)]
        public static extern int DwmUpdateThumbnailProperties(IntPtr hThumb, ref DWM_THUMBNAIL_PROPERTIES props);
    }
}
