using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Thumbs
{
    public partial class MainWindow : Window
    {
        WindowInteropHelper WIH;
        IntPtr ThumbHandle;

        public ObservableCollection<KeyValuePair<string, IntPtr>> AvailableWindows { get; private set; }

        public static DependencyProperty SelectedWindowProperty =
            DependencyProperty.Register("SelectedWindow",
                                        typeof(IntPtr),
                                        typeof(MainWindow),
                                        new UIPropertyMetadata(new IntPtr(-1)));

        public IntPtr SelectedWindow
        {
            get { return (IntPtr)GetValue(SelectedWindowProperty); }
            set { SetValue(SelectedWindowProperty, value); }
        }
        
        public MainWindow()
        {
            InitializeComponent();

            AvailableWindows = new ObservableCollection<KeyValuePair<string, IntPtr>>();

            DataContext = this;

            WIH = new WindowInteropHelper(this);

            Loaded += (s, e) => RefreshWindows();
            OpacitySlider.ValueChanged += (s, e) => UpdateThumb();
            SizeChanged += (s, e) => UpdateThumb();
            
            WindowBox.SelectionChanged += (s, e) =>
                {
                    if (WindowBox.SelectedIndex != -1)
                    {
                        if (ThumbHandle != IntPtr.Zero)
                            DWMApi.DwmUnregisterThumbnail(ThumbHandle);

                        if (DWMApi.DwmRegisterThumbnail(WIH.Handle, SelectedWindow, out ThumbHandle) == 0)
                            UpdateThumb();
                    }
                    else WindowBox.SelectedIndex = 0;
                };

            CommandBindings.Add(new CommandBinding(NavigationCommands.Refresh, (s, e) =>
                {
                    if (ThumbHandle != IntPtr.Zero)
                        DWMApi.DwmUnregisterThumbnail(ThumbHandle);

                    RefreshWindows();
                }));
        }

        void RefreshWindows()
        {
            AvailableWindows.Clear();

            User32.EnumWindows((hwnd, e) =>
                {
                    if (WIH.Handle != hwnd && (User32.GetWindowLongA(hwnd, User32.GWL_STYLE) & User32.TARGETWINDOW) == User32.TARGETWINDOW)
                    {
                        var sb = new StringBuilder(100);
                        User32.GetWindowText(hwnd, sb, sb.Capacity);

                        AvailableWindows.Add(new KeyValuePair<string, IntPtr>(sb.ToString(), hwnd));
                    }

                    return true;
                }, 0);

            SelectedWindow = AvailableWindows[0].Value;
        }

        void UpdateThumb()
        {
            if (ThumbHandle != IntPtr.Zero)
            {
                PSIZE size;
                DWMApi.DwmQueryThumbnailSourceSize(ThumbHandle, out size);

                DWM_THUMBNAIL_PROPERTIES props = new DWM_THUMBNAIL_PROPERTIES()
                {
                    fVisible = true,
                    dwFlags = DWMApi.DWM_TNP_VISIBLE | DWMApi.DWM_TNP_RECTDESTINATION | DWMApi.DWM_TNP_OPACITY,
                    opacity = (byte)OpacitySlider.Value,
                    rcDestination = new Rect(5, 30, (int)ActualWidth - 20, (int)ActualHeight - 60)
                };

                if (size.x < ActualWidth)
                    props.rcDestination.Right = props.rcDestination.Left + size.x;

                if (size.y < ActualHeight)
                    props.rcDestination.Bottom = props.rcDestination.Top + size.y;

                DWMApi.DwmUpdateThumbnailProperties(ThumbHandle, ref props);
            }
        }
    }
}
