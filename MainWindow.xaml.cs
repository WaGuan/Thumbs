using System.Windows.Interop;

namespace Thumbs
{
    public partial class MainWindow
    {
        readonly WindowInteropHelper _wih;
        
        Rect GetRect()
        {
            return new Rect(5, 40, (int)ActualWidth - 20, (int)ActualHeight - 50);
        }

        public MainWindow()
        {
            InitializeComponent();

            _wih = new WindowInteropHelper(this);

            var viewModel = DataContext as MainViewModel;
            
            Loaded += (s, e) => viewModel.Init(_wih.Handle, GetRect());

            SizeChanged += (s, e) => viewModel.SizeChanged(GetRect());
        }
    }
}
