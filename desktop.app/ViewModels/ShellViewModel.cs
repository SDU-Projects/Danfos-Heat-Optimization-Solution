using CommunityToolkit.Mvvm.Input;

namespace desktop.app.ViewModels
{
    public sealed class ShellViewModel : ViewModelBase
    {
        private string _selectedModule = "scenarios";

        public ShellViewModel()
        {
            ScenarioManager = new ScenarioManagerViewModel();
            NavigateToCommand = new RelayCommand<string>(NavigateTo);
        }

        public ScenarioManagerViewModel ScenarioManager { get; }

        public RelayCommand<string> NavigateToCommand { get; }

        public bool IsAssetsSelected => _selectedModule == "assets";

        public bool IsSourceDataSelected => _selectedModule == "source-data";

        public bool IsScenarioManagerSelected => _selectedModule == "scenarios";

        public bool IsRunOptimizationSelected => _selectedModule == "run-optimization";

        public bool IsResultsSelected => _selectedModule == "results";

        private void NavigateTo(string? module)
        {
            if (string.IsNullOrWhiteSpace(module) || module == _selectedModule)
            {
                return;
            }

            _selectedModule = module;
            OnPropertyChanged(nameof(IsAssetsSelected));
            OnPropertyChanged(nameof(IsSourceDataSelected));
            OnPropertyChanged(nameof(IsScenarioManagerSelected));
            OnPropertyChanged(nameof(IsRunOptimizationSelected));
            OnPropertyChanged(nameof(IsResultsSelected));
        }
    }
}
