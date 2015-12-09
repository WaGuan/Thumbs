using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace Thumbs
{
    public partial class MainWindow : Window
    {
        WindowInteropHelper WIH;
        IntPtr ThumbHandle;

        public MainWindow()
        {
            InitializeComponent();

            WIH = new WindowInteropHelper(this);

            Loaded += (s, e) => GetWindows();
            OpacitySlider.ValueChanged += (s, e) => UpdateThumb();
            SizeChanged += (s, e) => UpdateThumb();

            CommandBindings.Add(new CommandBinding(NavigationCommands.Refresh, (s, e) => Refresh()));
        }

        #region Retrieve list of windows
        List<KeyValuePair<string, IntPtr>> AvailableWindows;

        void GetWindows()
        {
            AvailableWindows = new List<KeyValuePair<string, IntPtr>>();
            User32.EnumWindows(Callback, 0);

            WindowBox.Items.Clear();

            foreach (var w in AvailableWindows)
                WindowBox.Items.Add(w);

            WindowBox.SelectedIndex = 0;
        }

        bool Callback(IntPtr hwnd, int lParam)
        {
            if (WIH.Handle != hwnd && (User32.GetWindowLongA(hwnd, User32.GWL_STYLE) & User32.TARGETWINDOW) == User32.TARGETWINDOW)
            {
                var sb = new StringBuilder(100);
                User32.GetWindowText(hwnd, sb, sb.Capacity);

                AvailableWindows.Add(new KeyValuePair<string, IntPtr>(sb.ToString(), hwnd));
            }

            return true; //continue enumeration
        }
        #endregion

        void Refresh()
        {
            if (ThumbHandle != IntPtr.Zero)
                DWMApi.DwmUnregisterThumbnail(ThumbHandle);

            GetWindows();
        }

        void SelectedWindowChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WindowBox.SelectedIndex != -1)
            {
                var w = (KeyValuePair<string, IntPtr>)WindowBox.SelectedItem;

                if (ThumbHandle != IntPtr.Zero)
                    DWMApi.DwmUnregisterThumbnail(ThumbHandle);

                int i = DWMApi.DwmRegisterThumbnail(WIH.Handle, w.Value, out ThumbHandle);

                if (i == 0)
                    UpdateThumb();
            }
        }

        void UpdateThumb()
        {
            if (ThumbHandle != IntPtr.Zero)
            {
                PSIZE size;
                DWMApi.DwmQueryThumbnailSourceSize(ThumbHandle, out size);

                DWM_THUMBNAIL_PROPERTIES props = new DWM_THUMBNAIL_PROPERTIES();

                props.fVisible = true;
                props.dwFlags = DWMApi.DWM_TNP_VISIBLE | DWMApi.DWM_TNP_RECTDESTINATION | DWMApi.DWM_TNP_OPACITY;
                props.opacity = (byte)OpacitySlider.Value;
                props.rcDestination = new Rect(5, 30, (int)Width - 20, (int)Height - 60);

                if (size.x < Width)
                    props.rcDestination.Right = props.rcDestination.Left + size.x;

                if (size.y < Height)
                    props.rcDestination.Bottom = props.rcDestination.Top + size.y;

                DWMApi.DwmUpdateThumbnailProperties(ThumbHandle, ref props);
            }
        }
    }
}
