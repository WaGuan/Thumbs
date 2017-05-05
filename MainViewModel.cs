using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace Thumbs
{
    class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            RefreshCommand = new DelegateCommand(RefreshWindows);
        }

        public ObservableCollection<KeyValuePair<string, IntPtr>> AvailableWindows { get; } = new ObservableCollection<KeyValuePair<string, IntPtr>>();

        IntPtr _selectedWindow = IntPtr.Zero;

        public IntPtr SelectedWindow
        {
            get { return _selectedWindow; }
            set
            {
                if (value == IntPtr.Zero)
                    return;

                _selectedWindow = value;
                
                if (_thumbHandle != IntPtr.Zero)
                    DWMApi.DwmUnregisterThumbnail(_thumbHandle);

                if (DWMApi.DwmRegisterThumbnail(_targetHandle, SelectedWindow, out _thumbHandle) == 0)
                    Update();

                OnPropertyChanged();
            }
        }

        IntPtr _targetHandle, _thumbHandle;
        Rect _targetRect;

        public void Init(IntPtr Target, Rect Location)
        {
            _targetHandle = Target;
            _targetRect = Location;
            
            RefreshWindows();
        }

        public void SizeChanged(Rect Location)
        {
            _targetRect = Location;

            Update();
        }
        
        public ICommand RefreshCommand { get; }

        void RefreshWindows()
        {
            if (_thumbHandle != IntPtr.Zero)
                DWMApi.DwmUnregisterThumbnail(_thumbHandle);

            AvailableWindows.Clear();

            User32.EnumWindows((hwnd, e) =>
            {
                if (_targetHandle != hwnd && (User32.GetWindowLongA(hwnd, User32.GWL_STYLE) & User32.TARGETWINDOW) == User32.TARGETWINDOW)
                {
                    var sb = new StringBuilder(100);
                    User32.GetWindowText(hwnd, sb, sb.Capacity);

                    var text = sb.ToString();

                    if (!string.IsNullOrWhiteSpace(text))
                        AvailableWindows.Add(new KeyValuePair<string, IntPtr>(text, hwnd));
                }

                return true;
            }, 0);
            
            if (AvailableWindows.Count > 0)
                SelectedWindow = AvailableWindows[0].Value;
        }

        public void Update()
        {
            if (_thumbHandle == IntPtr.Zero)
                return;

            DWMApi.DwmQueryThumbnailSourceSize(_thumbHandle, out PSIZE size);

            var props = new DWM_THUMBNAIL_PROPERTIES
            {
                fVisible = true,
                dwFlags = DWMApi.DWM_TNP_VISIBLE | DWMApi.DWM_TNP_RECTDESTINATION | DWMApi.DWM_TNP_OPACITY,
                opacity = 255,
                rcDestination = _targetRect
            };

            if (size.x < _targetRect.Width)
                props.rcDestination.Right = props.rcDestination.Left + size.x;

            if (size.y < _targetRect.Height)
                props.rcDestination.Bottom = props.rcDestination.Top + size.y;

            DWMApi.DwmUpdateThumbnailProperties(_thumbHandle, ref props);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string PropertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
