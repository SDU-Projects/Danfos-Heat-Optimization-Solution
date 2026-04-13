using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace desktop.app.ViewModels
{
    public sealed class ScenarioManagerViewModel : ViewModelBase
    {
        private string _newScenarioName = string.Empty;
        private string? _selectedDataset;
        private string? _selectedMaintenanceUnit;
        private bool _isCreateDialogOpen;

        public ScenarioManagerViewModel()
        {
            Scenarios = new ObservableCollection<ScenarioItem>
            {
                new ScenarioItem(
                    "winter 1",
                    "GB1, GB2, GB3, OB1, GM1, EB1",
                    "2026 Heat Production Optimization - Danfoss Deliveries - Source Data Manager (SDM) (2)")
            };

            AvailableDatasets = new ObservableCollection<string>
            {
                "No dataset selected",
                "2026 Heat Production Optimization - Danfoss Deliveries - Source Data Manager (SDM) (2)",
                "2025 Baseline Dispatch Data"
            };

            ProductionUnits = new ObservableCollection<UnitOption>
            {
                CreateUnit("GB1", "Gas Boiler"),
                CreateUnit("GB2", "Gas Boiler"),
                CreateUnit("GB3", "Gas Boiler"),
                CreateUnit("OB1", "Oil Boiler"),
                CreateUnit("GM1", "Gas Motor"),
                CreateUnit("EB1", "Electric Boiler")
            };

            MaintenanceUnitOptions = new ObservableCollection<string>
            {
                "No maintenance",
                "GB1",
                "GB2",
                "GB3",
                "OB1",
                "GM1",
                "EB1"
            };

            SelectedDataset = AvailableDatasets[0];
            SelectedMaintenanceUnit = MaintenanceUnitOptions[0];

            OpenCreateScenarioCommand = new RelayCommand(OpenCreateDialog);
            CancelCreateScenarioCommand = new RelayCommand(CancelCreateDialog);
            CreateScenarioCommand = new RelayCommand(CreateScenario, () => CanCreateScenario);
            EditScenarioCommand = new RelayCommand<ScenarioItem>(EditScenario);
            DeleteScenarioCommand = new RelayCommand<ScenarioItem>(DeleteScenario);
        }

        public ObservableCollection<ScenarioItem> Scenarios { get; }

        public ObservableCollection<UnitOption> ProductionUnits { get; }

        public ObservableCollection<string> AvailableDatasets { get; }

        public ObservableCollection<string> MaintenanceUnitOptions { get; }

        public RelayCommand OpenCreateScenarioCommand { get; }

        public RelayCommand CancelCreateScenarioCommand { get; }

        public RelayCommand CreateScenarioCommand { get; }

        public RelayCommand<ScenarioItem> EditScenarioCommand { get; }

        public RelayCommand<ScenarioItem> DeleteScenarioCommand { get; }

        public bool IsCreateDialogOpen
        {
            get => _isCreateDialogOpen;
            set => SetProperty(ref _isCreateDialogOpen, value);
        }

        public string NewScenarioName
        {
            get => _newScenarioName;
            set
            {
                if (SetProperty(ref _newScenarioName, value))
                {
                    CreateScenarioCommand?.NotifyCanExecuteChanged();
                }
            }
        }

        public string? SelectedDataset
        {
            get => _selectedDataset;
            set
            {
                if (SetProperty(ref _selectedDataset, value))
                {
                    CreateScenarioCommand?.NotifyCanExecuteChanged();
                }
            }
        }

        public string? SelectedMaintenanceUnit
        {
            get => _selectedMaintenanceUnit;
            set => SetProperty(ref _selectedMaintenanceUnit, value);
        }

        public bool CanCreateScenario =>
            !string.IsNullOrWhiteSpace(NewScenarioName)
            && !string.Equals(SelectedDataset, "No dataset selected", StringComparison.Ordinal)
            && ProductionUnits.Any(unit => unit.IsSelected);

        private UnitOption CreateUnit(string code, string type)
        {
            var option = new UnitOption(code, type);
            option.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(UnitOption.IsSelected))
                {
                    CreateScenarioCommand?.NotifyCanExecuteChanged();
                }
            };

            return option;
        }

        private void OpenCreateDialog()
        {
            IsCreateDialogOpen = true;
        }

        private void CancelCreateDialog()
        {
            IsCreateDialogOpen = false;
            ResetDialogState();
        }

        private void CreateScenario()
        {
            var selectedUnits = string.Join(", ",
                ProductionUnits.Where(unit => unit.IsSelected).Select(unit => unit.Code));

            Scenarios.Insert(0, new ScenarioItem(NewScenarioName.Trim(), selectedUnits, SelectedDataset ?? string.Empty));
            IsCreateDialogOpen = false;
            ResetDialogState();
        }

        private void EditScenario(ScenarioItem? item)
        {
            if (item is null)
            {
                return;
            }

            NewScenarioName = item.Name;
            SelectedDataset = item.Dataset;
            IsCreateDialogOpen = true;
        }

        private void DeleteScenario(ScenarioItem? item)
        {
            if (item is null)
            {
                return;
            }

            Scenarios.Remove(item);
        }

        private void ResetDialogState()
        {
            NewScenarioName = string.Empty;
            SelectedDataset = AvailableDatasets[0];
            SelectedMaintenanceUnit = MaintenanceUnitOptions[0];

            foreach (var unit in ProductionUnits)
            {
                unit.IsSelected = true;
            }
        }
    }

    public sealed class ScenarioItem
    {
        public ScenarioItem(string name, string activeUnits, string dataset)
        {
            Name = name;
            ActiveUnits = activeUnits;
            Dataset = dataset;
        }

        public string Name { get; }

        public string ActiveUnits { get; }

        public string Dataset { get; }
    }

    public sealed partial class UnitOption : ObservableObject
    {
        public UnitOption(string code, string type)
        {
            Code = code;
            Type = type;
            IsSelected = true;
        }

        public string Code { get; }

        public string Type { get; }

        [ObservableProperty]
        private bool _isSelected;
    }
}
