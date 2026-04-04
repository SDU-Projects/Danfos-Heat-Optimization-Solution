using Avalonia.Controls;
using desktop.app.ViewModels;

namespace desktop.app.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext= new MainWindowViewModel();
        }
    }
}