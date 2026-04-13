namespace desktop.app.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public ShellViewModel Shell { get; } = new ShellViewModel();
    }
}
