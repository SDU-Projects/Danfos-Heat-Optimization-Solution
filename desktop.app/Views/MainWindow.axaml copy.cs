using Avalonia.Controls;

namespace desktop.app.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext= new MainViewModel();
        }
    }
}