using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using data.Entities;
using data.Models.Base;
using data.Services;
using ScottPlot;

namespace desktop.app.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly Dictionary<string, List<SourceDataPoint>> _sourceDataRowsByDatasetName;
        private readonly AssetService _assetService;
        private readonly OptimizationService _optimizationService;

        public AsyncRelayCommand AddProductionUnitCommand { get; }

        public MainWindowViewModel()
        {
            DebugMessage = "MainWindowViewModel constructor";
            _assetService = new AssetService();
            _optimizationService = new OptimizationService();
            _sourceDataRowsByDatasetName = new Dictionary<string, List<SourceDataPoint>>(StringComparer.OrdinalIgnoreCase);
            ProductionUnits = new ObservableCollection<ProductionUnit>();
            SourceDatasets = new ObservableCollection<SourceDatasetSummary>();
            SelectedDatasetRows = new ObservableCollection<SourceDataPoint>();
            OptimizationRuns = new ObservableCollection<OptimizationRunDto>();
            LoadProductionUnitsAsync();
            SelectedProductionUnitType = ProductionUnitTypes.First();

            AddProductionUnitCommand = new AsyncRelayCommand(AddProductionUnitAsync);
            DebugMessage += $"\nAddProductionUnitCommand is null: {AddProductionUnitCommand == null}";

            ShowCatalog();
        }

        [ObservableProperty]
        public ObservableCollection<ProductionUnit> productionUnits;

        [ObservableProperty]
        public ObservableCollection<SourceDatasetSummary> sourceDatasets;

        [ObservableProperty]
        public ObservableCollection<SourceDataPoint> selectedDatasetRows;

        [ObservableProperty]
        public ProductionUnit? selectedUnit;

        [ObservableProperty]
        private SourceDatasetSummary? selectedSourceDataset;

        [ObservableProperty]
        private bool isCatalogVisible = true;

        [ObservableProperty]
        private bool isCreateVisible;

        [ObservableProperty]
        private bool isSourceDataVisible;

        [ObservableProperty]
        private bool isScenariosVisible;

        [ObservableProperty]
        private bool isRunOptimizationVisible;

        [ObservableProperty]
        private bool isVisualizationVisible;

        [ObservableProperty]
        private bool isUpdateVisible;

        [ObservableProperty]
        private bool isDeleteVisible;

        [ObservableProperty]
        private string searchTerm = string.Empty;

        [ObservableProperty]
        private string newUnitName = string.Empty;

        [ObservableProperty]
        private string newUnitImageUrl = string.Empty;

        [ObservableProperty]
        private double newUnitMaxHeatMW;

        [ObservableProperty]
        private double newUnitProductionCostPerMWh;

        [ObservableProperty]
        private double newUnitCO2KgPerMWh;

        [ObservableProperty]
        private string debugMessage = string.Empty;

        [ObservableProperty]
        private string sourceDataStatusMessage = "No dataset loaded yet.";

        [ObservableProperty]
        private string sourceDataValidationMessage = string.Empty;

        public string SelectedProductionUnitType { get; set; } = "GasBoiler";

        [ObservableProperty]
        private bool isLoading;

        public ObservableCollection<string> ProductionUnitTypes { get; } = new ObservableCollection<string>
        {
            nameof(ProductionUnitType.ElectricBoiler),
            nameof(ProductionUnitType.GasBoiler),
            nameof(ProductionUnitType.GasMotor),
            nameof(ProductionUnitType.OilBoiler)
        };

        public IEnumerable<ProductionUnit> FilteredProductionUnits =>
            string.IsNullOrWhiteSpace(SearchTerm)
            ? ProductionUnits
            : ProductionUnits.Where(x => x.Data.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));

        public bool HasSourceDataValidationMessage
        {
            get
            {
                return !string.IsNullOrWhiteSpace(SourceDataValidationMessage);
            }
        }

        public bool HasSelectedSourceDataset
        {
            get
            {
                return SelectedSourceDataset != null;
            }
        }

        public string SelectedDatasetDateRange
        {
            get
            {
                if (SelectedSourceDataset == null)
                {
                    return "-";
                }

                return SelectedSourceDataset.DateRange;
            }
        }

        private async void LoadProductionUnitsAsync()
        {
            IsLoading = true;
            try
            {
                var data = await _assetService.GetAllAssetsAsync();
                ProductionUnits.Clear();
                foreach (var item in data)
                {
                    if (!string.IsNullOrWhiteSpace(item.Data.ImageUrl) && !item.Data.ImageUrl.Contains("://"))
                    {
                        item.Data.ImageUrl = $"avares://desktop.app/Images/{item.Data.ImageUrl}";
                    }
                    ProductionUnits.Add(item);
                }
                OnPropertyChanged(nameof(FilteredProductionUnits));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar ProductionUnits: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void ShowCatalog()
        {
            SearchTerm = string.Empty;
            IsCatalogVisible = true;
            IsCreateVisible = false;
            IsSourceDataVisible = false;
            IsScenariosVisible = false;
            IsRunOptimizationVisible = false;
            IsVisualizationVisible = false;
            IsUpdateVisible = false;
            IsDeleteVisible = false;
        }

        [RelayCommand]
        public void ShowCreate()
        {
            DebugMessage += "\nShowCreate called";
            IsCatalogVisible = false;
            IsCreateVisible = true;
            IsSourceDataVisible = false;
            IsScenariosVisible = false;
            IsRunOptimizationVisible = false;
            IsVisualizationVisible = false;
            IsUpdateVisible = false;
            IsDeleteVisible = false;

            ClearForm();
        }

        [RelayCommand]
        public void ShowSourceData()
        {
            IsCatalogVisible = false;
            IsCreateVisible = false;
            IsSourceDataVisible = true;
            IsScenariosVisible = false;
            IsRunOptimizationVisible = false;
            IsVisualizationVisible = false;
            IsUpdateVisible = false;
            IsDeleteVisible = false;
        }

        [RelayCommand]
        private void GenerateWinterSample()
        {
            List<SourceDataPoint> rows = BuildGeneratedRows(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), true);
            AddOrReplaceDatasetFromRows("Sample Winter Data", "Winter", rows, true);
        }

        [RelayCommand]
        private void GenerateSummerSample()
        {
            List<SourceDataPoint> rows = BuildGeneratedRows(new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc), false);
            AddOrReplaceDatasetFromRows("Sample Summer Data", "Summer", rows, true);
        }

        [RelayCommand]
        private void DeleteSourceDataset(SourceDatasetSummary? dataset)
        {
            if (dataset == null)
            {
                return;
            }

            _sourceDataRowsByDatasetName.Remove(dataset.DatasetName);
            SourceDatasets.Remove(dataset);

            if (SelectedSourceDataset != null && string.Equals(SelectedSourceDataset.DatasetName, dataset.DatasetName, StringComparison.OrdinalIgnoreCase))
            {
                SelectedSourceDataset = null;
                SelectedDatasetRows.Clear();
            }

            SourceDataValidationMessage = string.Empty;
            SourceDataStatusMessage = $"Dataset '{dataset.DatasetName}' deleted.";
            OnPropertyChanged(nameof(HasSelectedSourceDataset));
            OnPropertyChanged(nameof(SelectedDatasetDateRange));
            OnPropertyChanged(nameof(HasSourceDataValidationMessage));
        }

        [RelayCommand]
        public void ShowScenarios()
        {
            IsCatalogVisible = false;
            IsCreateVisible = false;
            IsSourceDataVisible = false;
            IsScenariosVisible = true;
            IsRunOptimizationVisible = false;
            IsVisualizationVisible = false;
            IsUpdateVisible = false;
            IsDeleteVisible = false;
        }

        [RelayCommand]
        public void ShowRunOptimization()
        {
            IsCatalogVisible = false;
            IsCreateVisible = false;
            IsSourceDataVisible = false;
            IsScenariosVisible = false;
            IsRunOptimizationVisible = true;
            IsVisualizationVisible = false;
            IsUpdateVisible = false;
            IsDeleteVisible = false;
        }

        [RelayCommand]
        public void ShowVisualization()
        {
            IsCatalogVisible = false;
            IsCreateVisible = false;
            IsSourceDataVisible = false;
            IsScenariosVisible = false;
            IsRunOptimizationVisible = false;
            IsVisualizationVisible = true;
            IsUpdateVisible = false;
            IsDeleteVisible = false;
        }

        private async Task AddProductionUnitAsync()
        {
            DebugMessage += "\nCreate button clicked";
            DebugMessage += $"\nName: '{NewUnitName}', Image: '{NewUnitImageUrl}'";
            if (string.IsNullOrWhiteSpace(NewUnitName) || string.IsNullOrWhiteSpace(NewUnitImageUrl))
            {
                DebugMessage += "\nValidation failed";
                return;
            }
            DebugMessage += "\nValidation passed";

            IsLoading = true;
            try
            {
                var type = Enum.Parse<ProductionUnitType>(SelectedProductionUnitType);
                ProductionUnitBase baseUnit = type switch
                {
                    ProductionUnitType.ElectricBoiler => new data.Models.ElectricBoiler { Name = string.Empty, ImageUrl = string.Empty },
                    ProductionUnitType.GasBoiler => new data.Models.GasBoiler { Name = string.Empty, ImageUrl = string.Empty },
                    ProductionUnitType.GasMotor => new data.Models.GasMotor { Name = string.Empty, ImageUrl = string.Empty },
                    ProductionUnitType.OilBoiler => new data.Models.OilBoiler { Name = string.Empty, ImageUrl = string.Empty },
                    _ => new data.Models.ElectricBoiler { Name = string.Empty, ImageUrl = string.Empty }
                };

                baseUnit.Name = NewUnitName;
                baseUnit.ImageUrl = NewUnitImageUrl;
                baseUnit.MaxHeatMW = NewUnitMaxHeatMW;
                baseUnit.ProductionCostPerMWh = NewUnitProductionCostPerMWh;
                baseUnit.CO2KgPerMWh = NewUnitCO2KgPerMWh;
                baseUnit.IsAvailable = true;

                var newAsset = new ProductionUnit
                {
                    Data = baseUnit,
                    Type = type
                };

                var requestOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() },
                    TypeInfoResolver = new DefaultJsonTypeInfoResolver()
                };
                var requestJson = JsonSerializer.Serialize(newAsset, requestOptions);
                DebugMessage += $"\nRequest JSON: {requestJson}";
                DebugMessage += "\nAbout to call API";
                var created = await _assetService.CreateAssetAsync(newAsset);
                if (created != null)
                {
                    if (!string.IsNullOrWhiteSpace(created.Data.ImageUrl) && !created.Data.ImageUrl.Contains("://"))
                    {
                        created.Data.ImageUrl = $"avares://desktop.app/Images/{created.Data.ImageUrl}";
                    }
                    ProductionUnits.Add(created);
                    OnPropertyChanged(nameof(FilteredProductionUnits));
                    DebugMessage += "\nUnit created successfully";
                    ShowCatalog();
                }
                else
                {
                    DebugMessage += "\nAPI returned null";
                }
            }
            catch (Exception ex)
            {
                DebugMessage += $"\nError: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task DeleteProductionUnitAsync(ProductionUnit unit)
        {
            if (unit == null)
                return;

            IsLoading = true;
            try
            {
                var ok = await _assetService.DeleteAssetAsync(unit.Id);
                if (ok)
                {
                    ProductionUnits.Remove(unit);
                    OnPropertyChanged(nameof(FilteredProductionUnits));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error borrando ProductionUnit: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task UpdateProductionUnitAsync(ProductionUnit unit)
        {
            if (unit == null)
                return;

            IsLoading = true;
            try
            {
                var ok = await _assetService.UpdateAssetAsync(unit.Id, unit);
                if (!ok)
                {
                    DebugMessage += "\nUpdate failed.";
                }
                else
                {
                    OnPropertyChanged(nameof(FilteredProductionUnits));
                    DebugMessage += "\nUnit updated successfully.";
                }
            }
            catch (Exception ex)
            {
                DebugMessage += $"\nError updating ProductionUnit: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ── Optimization Run view ────────────────────────────────────────────────

        [ObservableProperty]
        private string selectedSeason = "Winter";

        public ObservableCollection<string> SeasonOptions { get; } = new ObservableCollection<string> { "Winter", "Summer" };

        [ObservableProperty]
        private string optimizationStatusMessage = "Configure settings and click Run.";

        [ObservableProperty]
        private ObservableCollection<OptimizationRunDto> optimizationRuns;

        [ObservableProperty]
        private OptimizationRunDto? selectedOptimizationRun;

        [RelayCommand]
        private async Task RunOptimizationAsync()
        {
            IsLoading = true;
            OptimizationStatusMessage = "Running optimization...";
            try
            {
                var request = new OptimizationRunRequest
                {
                    Season = SelectedSeason,
                    Objective = 0, // Cost
                    CostWeight = 1m,
                    Co2Weight = 0m,
                    ElectricityPriceSource = 0
                };

                OptimizationRunDto result = await _optimizationService.RunOptimizationAsync(request);
                OptimizationRuns.Insert(0, result);
                SelectedOptimizationRun = result;
                OptimizationStatusMessage = $"Run #{result.Id} completed. Total cost: {result.TotalNetCostDkk:N0} DKK, CO₂: {result.TotalCo2Kg:N0} kg";
            }
            catch (Exception ex)
            {
                OptimizationStatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task LoadOptimizationRunsAsync()
        {
            IsLoading = true;
            try
            {
                var runs = await _optimizationService.GetRunsAsync();
                OptimizationRuns.Clear();
                foreach (var r in runs) OptimizationRuns.Add(r);
                if (OptimizationRuns.Count > 0)
                    SelectedOptimizationRun = OptimizationRuns[0];
                OptimizationStatusMessage = $"Loaded {runs.Count} run(s) from history.";
            }
            catch (Exception ex)
            {
                OptimizationStatusMessage = $"Error loading runs: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ── Visualization ────────────────────────────────────────────────────────

        private static readonly ScottPlot.Color[] ChartPalette =
        {
            new ScottPlot.Color(30, 130, 210),
            new ScottPlot.Color(255, 160, 30),
            new ScottPlot.Color(80, 190, 100),
            new ScottPlot.Color(220, 60, 60),
            new ScottPlot.Color(160, 100, 200),
            new ScottPlot.Color(80, 200, 200)
        };

        public bool HasCharts => SelectedOptimizationRun?.Hours?.Count > 0;

        partial void OnSelectedOptimizationRunChanged(OptimizationRunDto? value)
        {
            OnPropertyChanged(nameof(HasCharts));
            if (value != null && value.Hours.Count == 0 && value.Id > 0)
            {
                _ = LoadFullRunAndBuildChartsAsync(value.Id);
            }
        }

        private async Task LoadFullRunAndBuildChartsAsync(int runId)
        {
            try
            {
                var fullRun = await _optimizationService.GetRunAsync(runId);
                if (fullRun != null)
                {
                    var existing = OptimizationRuns.FirstOrDefault(r => r.Id == runId);
                    if (existing != null)
                    {
                        int idx = OptimizationRuns.IndexOf(existing);
                        OptimizationRuns[idx] = fullRun;
                        SelectedOptimizationRun = fullRun;
                    }
                }
            }
            catch (Exception ex)
            {
                OptimizationStatusMessage = $"Error loading run detail: {ex.Message}";
            }
        }

        public void ConfigureHeatProductionChart(ScottPlot.Plot plt)
        {
            plt.Clear();
            if (SelectedOptimizationRun == null) return;
            var hours = SelectedOptimizationRun.Hours.OrderBy(h => h.TimeFromUtc).ToList();
            var unitNames = GetUnitNames(hours);
            double[] xs = hours.Select(h => h.TimeFromUtc.ToOADate()).ToArray();
            for (int i = 0; i < unitNames.Count; i++)
            {
                string name = unitNames[i];
                double[] ys = hours.Select(h => (double)(h.UnitResults.FirstOrDefault(u => u.UnitName == name)?.HeatProducedMWh ?? 0m)).ToArray();
                var s = plt.Add.Scatter(xs, ys);
                s.Label = name;
                s.Color = ChartPalette[i % ChartPalette.Length];
                s.LineWidth = 1.5f;
                s.MarkerSize = 0;
            }
            double[] demandYs = hours.Select(h => (double)h.HeatDemandMWh).ToArray();
            var demand = plt.Add.Scatter(xs, demandYs);
            demand.Label = "Heat Demand";
            demand.Color = new ScottPlot.Color(0, 0, 0);
            demand.LineWidth = 2f;
            demand.LinePattern = ScottPlot.LinePattern.Dashed;
            demand.MarkerSize = 0;
            plt.Axes.DateTimeTicksBottom();
            plt.Title("Heat Production per Unit (MWh)");
            plt.XLabel("Time (UTC)");
            plt.YLabel("MWh");
            plt.ShowLegend();
        }

        public void ConfigureElectricityChart(ScottPlot.Plot plt)
        {
            plt.Clear();
            if (SelectedOptimizationRun == null) return;
            var hours = SelectedOptimizationRun.Hours.OrderBy(h => h.TimeFromUtc).ToList();
            var unitNames = GetUnitNames(hours);
            double[] xs = hours.Select(h => h.TimeFromUtc.ToOADate()).ToArray();
            var rightAxis = plt.Axes.AddRightAxis();
            rightAxis.Label.Text = "DKK/MWh";
            for (int i = 0; i < unitNames.Count; i++)
            {
                string name = unitNames[i];
                double[] ys = hours.Select(h => (double)(h.UnitResults.FirstOrDefault(u => u.UnitName == name)?.ElectricityMWh ?? 0m)).ToArray();
                var s = plt.Add.Scatter(xs, ys);
                s.Label = name;
                s.Color = ChartPalette[i % ChartPalette.Length];
                s.LineWidth = 1.5f;
                s.MarkerSize = 0;
            }
            double[] priceYs = hours.Select(h => (double)h.ElectricityPriceDkkPerMWh).ToArray();
            var price = plt.Add.Scatter(xs, priceYs);
            price.Label = "Electricity Price (DKK/MWh)";
            price.Color = new ScottPlot.Color(150, 150, 150);
            price.LinePattern = ScottPlot.LinePattern.Dashed;
            price.LineWidth = 1.5f;
            price.MarkerSize = 0;
            price.Axes.YAxis = rightAxis;
            plt.Axes.DateTimeTicksBottom();
            plt.Title("Electricity Production/Consumption (MWh) & Price (DKK/MWh)");
            plt.XLabel("Time (UTC)");
            plt.YLabel("MWh");
            plt.ShowLegend();
        }

        public void ConfigureCo2Chart(ScottPlot.Plot plt)
        {
            plt.Clear();
            if (SelectedOptimizationRun == null) return;
            var hours = SelectedOptimizationRun.Hours.OrderBy(h => h.TimeFromUtc).ToList();
            var unitNames = GetUnitNames(hours);
            double[] xs = hours.Select(h => h.TimeFromUtc.ToOADate()).ToArray();
            for (int i = 0; i < unitNames.Count; i++)
            {
                string name = unitNames[i];
                double[] ys = hours.Select(h => (double)(h.UnitResults.FirstOrDefault(u => u.UnitName == name)?.Co2Kg ?? 0m)).ToArray();
                var s = plt.Add.Scatter(xs, ys);
                s.Label = name;
                s.Color = ChartPalette[i % ChartPalette.Length];
                s.LineWidth = 1.5f;
                s.MarkerSize = 0;
            }
            plt.Axes.DateTimeTicksBottom();
            plt.Title("CO₂ Emissions per Unit (kg)");
            plt.XLabel("Time (UTC)");
            plt.YLabel("kg");
            plt.ShowLegend();
        }

        public void ConfigureCostChart(ScottPlot.Plot plt)
        {
            plt.Clear();
            if (SelectedOptimizationRun == null) return;
            var hours = SelectedOptimizationRun.Hours.OrderBy(h => h.TimeFromUtc).ToList();
            double[] xs = hours.Select(h => h.TimeFromUtc.ToOADate()).ToArray();
            double[] costYs = hours.Select(h => (double)h.TotalNetCostDkk).ToArray();
            double[] cashYs = hours.Select(h => (double)h.ElectricityCashflowDkk).ToArray();
            var s1 = plt.Add.Scatter(xs, costYs);
            s1.Label = "Net Cost";
            s1.Color = new ScottPlot.Color(30, 130, 210);
            s1.LineWidth = 2f;
            s1.MarkerSize = 0;
            var s2 = plt.Add.Scatter(xs, cashYs);
            s2.Label = "Electricity Cashflow";
            s2.Color = new ScottPlot.Color(255, 160, 30);
            s2.LineWidth = 2f;
            s2.LinePattern = ScottPlot.LinePattern.Dashed;
            s2.MarkerSize = 0;
            plt.Axes.DateTimeTicksBottom();
            plt.Title("Net Cost (DKK) & Electricity Cashflow (DKK) per Hour");
            plt.XLabel("Time (UTC)");
            plt.YLabel("DKK");
            plt.ShowLegend();
        }

        public void ConfigureNetProductionCostChart(ScottPlot.Plot plt)
        {
            plt.Clear();
            if (SelectedOptimizationRun == null) return;
            var hours = SelectedOptimizationRun.Hours.OrderBy(h => h.TimeFromUtc).ToList();
            var unitNames = GetUnitNames(hours);
            double[] xs = hours.Select(h => h.TimeFromUtc.ToOADate()).ToArray();
            for (int i = 0; i < unitNames.Count; i++)
            {
                string name = unitNames[i];
                double[] ys = hours.Select(h => (double)(h.UnitResults.FirstOrDefault(u => u.UnitName == name)?.ScorePerMWh ?? 0m)).ToArray();
                var s = plt.Add.Scatter(xs, ys);
                s.Label = name;
                s.Color = ChartPalette[i % ChartPalette.Length];
                s.LineWidth = 1.5f;
                s.MarkerSize = 0;
            }
            plt.Axes.DateTimeTicksBottom();
            plt.Title("Net Production Cost per Unit per Hour (DKK/MWh)");
            plt.XLabel("Time (UTC)");
            plt.YLabel("DKK/MWh");
            plt.ShowLegend();
        }

        private static List<string> GetUnitNames(List<OptimizationHourResultDto> hours) =>
            hours.SelectMany(h => h.UnitResults.Select(u => u.UnitName))
                 .Distinct()
                 .OrderBy(x => x)
                 .ToList();

        private void ClearForm()        {
            NewUnitName = string.Empty;
            NewUnitImageUrl = string.Empty;
            NewUnitMaxHeatMW = 0;
            NewUnitProductionCostPerMWh = 0;
            NewUnitCO2KgPerMWh = 0;
            SelectedProductionUnitType = ProductionUnitTypes.First();
        }

        partial void OnSearchTermChanged(string value)
        {
            OnPropertyChanged(nameof(FilteredProductionUnits));
        }

        partial void OnSelectedSourceDatasetChanged(SourceDatasetSummary? value)
        {
            SelectedDatasetRows.Clear();

            if (value == null)
            {
                SourceDataStatusMessage = "Select a dataset to see its rows.";
                OnPropertyChanged(nameof(HasSelectedSourceDataset));
                OnPropertyChanged(nameof(SelectedDatasetDateRange));
                return;
            }

            if (_sourceDataRowsByDatasetName.TryGetValue(value.DatasetName, out List<SourceDataPoint>? rows))
            {
                foreach (SourceDataPoint row in rows)
                {
                    SelectedDatasetRows.Add(row);
                }

                SourceDataStatusMessage = $"Opened '{value.DatasetName}' ({rows.Count} rows).";
            }
            else
            {
                SourceDataStatusMessage = "No rows found for selected dataset.";
            }

            SourceDataValidationMessage = string.Empty;
            OnPropertyChanged(nameof(HasSelectedSourceDataset));
            OnPropertyChanged(nameof(SelectedDatasetDateRange));
            OnPropertyChanged(nameof(HasSourceDataValidationMessage));
        }

        public async Task ProcessCsvUploadAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                SourceDataValidationMessage = "No file selected.";
                SourceDataStatusMessage = "CSV upload failed.";
                OnPropertyChanged(nameof(HasSourceDataValidationMessage));
                return;
            }

            if (!File.Exists(filePath))
            {
                SourceDataValidationMessage = "Selected file does not exist.";
                SourceDataStatusMessage = "CSV upload failed.";
                OnPropertyChanged(nameof(HasSourceDataValidationMessage));
                return;
            }

            try
            {
                IsLoading = true;
                SourceDataValidationMessage = string.Empty;

                string[] lines = await File.ReadAllLinesAsync(filePath);
                ParseCsvResult parseResult = ParseCsvLines(lines);

                if (parseResult.HasErrors)
                {
                    SourceDataValidationMessage = parseResult.ErrorMessage;
                    SourceDataStatusMessage = "CSV upload failed.";
                    OnPropertyChanged(nameof(HasSourceDataValidationMessage));
                    return;
                }

                string datasetName = Path.GetFileNameWithoutExtension(filePath);
                string period = InferPeriod(parseResult.Rows);
                AddOrReplaceDatasetFromRows(datasetName, period, parseResult.Rows, false);
            }
            catch (Exception ex)
            {
                SourceDataValidationMessage = $"Upload failed: {ex.Message}";
                SourceDataStatusMessage = "CSV upload failed.";
                OnPropertyChanged(nameof(HasSourceDataValidationMessage));
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddOrReplaceDatasetFromRows(string datasetName, string period, List<SourceDataPoint> rows, bool generated)
        {
            if (rows.Count == 0)
            {
                SourceDataValidationMessage = "Dataset has no rows.";
                SourceDataStatusMessage = "Dataset creation failed.";
                OnPropertyChanged(nameof(HasSourceDataValidationMessage));
                return;
            }

            rows.Sort((left, right) => left.TimestampUtc.CompareTo(right.TimestampUtc));

            DateTime min = rows[0].TimestampUtc;
            DateTime max = rows[rows.Count - 1].TimestampUtc;

            SourceDatasetSummary summary = new SourceDatasetSummary
            {
                DatasetName = datasetName,
                Period = period,
                Hours = rows.Count,
                DateRange = $"{min:MMM d, HH:mm} - {max:MMM d, HH:mm}",
                Uploaded = DateTime.Now,
                Source = generated ? "Generated" : "Uploaded"
            };

            _sourceDataRowsByDatasetName[datasetName] = rows;

            SourceDatasetSummary? existing = SourceDatasets.FirstOrDefault(x => string.Equals(x.DatasetName, datasetName, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                int index = SourceDatasets.IndexOf(existing);
                SourceDatasets[index] = summary;
            }
            else
            {
                SourceDatasets.Insert(0, summary);
            }

            SelectedSourceDataset = summary;
            SourceDataValidationMessage = string.Empty;
            SourceDataStatusMessage = generated
                ? $"Generated '{datasetName}' with {rows.Count} rows."
                : $"Uploaded '{datasetName}' with {rows.Count} rows.";

            OnPropertyChanged(nameof(HasSourceDataValidationMessage));
        }

        private static ParseCsvResult ParseCsvLines(string[] lines)
        {
            if (lines.Length < 2)
            {
                return ParseCsvResult.Fail("CSV must include a header and at least one data row.");
            }

            string[] headers = SplitCsvLine(lines[0]);
            Dictionary<string, int> headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Length; i++)
            {
                string key = headers[i].Trim();
                if (!headerMap.ContainsKey(key))
                {
                    headerMap.Add(key, i);
                }
            }

            string[] requiredHeaders = new[] { "timestamp", "heat_demand", "electricity_price" };
            foreach (string header in requiredHeaders)
            {
                if (!headerMap.ContainsKey(header))
                {
                    return ParseCsvResult.Fail("CSV header must contain columns: timestamp, heat_demand, electricity_price.");
                }
            }

            List<SourceDataPoint> rows = new List<SourceDataPoint>();
            List<string> errors = new List<string>();

            for (int lineIndex = 1; lineIndex < lines.Length; lineIndex++)
            {
                string line = lines[lineIndex];
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] columns = SplitCsvLine(line);
                int timestampIndex = headerMap["timestamp"];
                int heatDemandIndex = headerMap["heat_demand"];
                int priceIndex = headerMap["electricity_price"];

                if (columns.Length <= Math.Max(timestampIndex, Math.Max(heatDemandIndex, priceIndex)))
                {
                    errors.Add($"Line {lineIndex + 1}: missing one or more required values.");
                    continue;
                }

                string timestampRaw = columns[timestampIndex].Trim();
                string heatDemandRaw = columns[heatDemandIndex].Trim();
                string priceRaw = columns[priceIndex].Trim();

                if (!DateTime.TryParse(timestampRaw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime timestamp))
                {
                    errors.Add($"Line {lineIndex + 1}: invalid timestamp '{timestampRaw}'.");
                    continue;
                }

                if (!decimal.TryParse(heatDemandRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal heatDemand))
                {
                    errors.Add($"Line {lineIndex + 1}: invalid heat_demand '{heatDemandRaw}'.");
                    continue;
                }

                if (!decimal.TryParse(priceRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal price))
                {
                    errors.Add($"Line {lineIndex + 1}: invalid electricity_price '{priceRaw}'.");
                    continue;
                }

                if (heatDemand < 0m)
                {
                    errors.Add($"Line {lineIndex + 1}: heat_demand must be non-negative.");
                    continue;
                }

                rows.Add(new SourceDataPoint
                {
                    TimestampUtc = timestamp,
                    HeatDemand = decimal.Round(heatDemand, 3),
                    ElectricityPrice = decimal.Round(price, 3)
                });
            }

            if (errors.Count > 0)
            {
                string joinedErrors = string.Join(Environment.NewLine, errors.Take(8));
                if (errors.Count > 8)
                {
                    joinedErrors += Environment.NewLine + $"... and {errors.Count - 8} more error(s).";
                }

                return ParseCsvResult.Fail(joinedErrors);
            }

            rows.Sort((left, right) => left.TimestampUtc.CompareTo(right.TimestampUtc));

            for (int i = 1; i < rows.Count; i++)
            {
                TimeSpan diff = rows[i].TimestampUtc - rows[i - 1].TimestampUtc;
                if (diff != TimeSpan.FromHours(1))
                {
                    return ParseCsvResult.Fail($"Timestamps must be hourly and consecutive. Gap found between {rows[i - 1].TimestampUtc:O} and {rows[i].TimestampUtc:O}.");
                }
            }

            return ParseCsvResult.Ok(rows);
        }

        private static string[] SplitCsvLine(string line)
        {
            List<string> values = new List<string>();
            bool inQuotes = false;
            int start = 0;

            for (int i = 0; i < line.Length; i++)
            {
                char current = line[i];
                if (current == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (current == ',' && !inQuotes)
                {
                    values.Add(UnwrapCsvValue(line.Substring(start, i - start)));
                    start = i + 1;
                }
            }

            values.Add(UnwrapCsvValue(line.Substring(start)));
            return values.ToArray();
        }

        private static string UnwrapCsvValue(string value)
        {
            string trimmed = value.Trim();
            if (trimmed.Length >= 2 && trimmed.StartsWith("\"") && trimmed.EndsWith("\""))
            {
                return trimmed.Substring(1, trimmed.Length - 2).Replace("\"\"", "\"");
            }

            return trimmed;
        }

        private static string InferPeriod(List<SourceDataPoint> rows)
        {
            if (rows.Count == 0)
            {
                return "Unknown";
            }

            int month = rows[0].TimestampUtc.Month;
            if (month == 12 || month == 1 || month == 2)
            {
                return "Winter";
            }

            if (month >= 6 && month <= 8)
            {
                return "Summer";
            }

            if (month >= 3 && month <= 5)
            {
                return "Spring";
            }

            return "Autumn";
        }

        private static List<SourceDataPoint> BuildGeneratedRows(DateTime startUtc, bool winterProfile)
        {
            List<SourceDataPoint> rows = new List<SourceDataPoint>();
            for (int hour = 0; hour < 168; hour++)
            {
                DateTime timestamp = startUtc.AddHours(hour);
                int hourOfDay = timestamp.Hour;

                decimal dayWave = Convert.ToDecimal(Math.Sin((hourOfDay / 24.0) * Math.PI * 2.0));
                decimal demandBase = winterProfile ? 8.8m : 3.2m;
                decimal demandAmplitude = winterProfile ? 1.3m : 0.55m;
                decimal priceBase = winterProfile ? 860m : 620m;
                decimal priceAmplitude = winterProfile ? 240m : 180m;

                decimal heatDemand = decimal.Round(demandBase + demandAmplitude * (0.5m + dayWave), 3);
                decimal electricityPrice = decimal.Round(priceBase + priceAmplitude * (0.5m + dayWave), 3);

                if (hourOfDay >= 17 && hourOfDay <= 20)
                {
                    electricityPrice += winterProfile ? 160m : 120m;
                }

                rows.Add(new SourceDataPoint
                {
                    TimestampUtc = timestamp,
                    HeatDemand = heatDemand,
                    ElectricityPrice = electricityPrice
                });
            }

            return rows;
        }

        public class SourceDatasetSummary
        {
            public string DatasetName { get; set; } = string.Empty;
            public string Period { get; set; } = string.Empty;
            public int Hours { get; set; }
            public string DateRange { get; set; } = string.Empty;
            public DateTime Uploaded { get; set; }
            public string Source { get; set; } = string.Empty;
        }

        public class SourceDataPoint
        {
            public DateTime TimestampUtc { get; set; }
            public decimal HeatDemand { get; set; }
            public decimal ElectricityPrice { get; set; }
        }

        private class ParseCsvResult
        {
            public bool HasErrors { get; set; }
            public string ErrorMessage { get; set; } = string.Empty;
            public List<SourceDataPoint> Rows { get; set; } = new List<SourceDataPoint>();

            public static ParseCsvResult Fail(string message)
            {
                return new ParseCsvResult
                {
                    HasErrors = true,
                    ErrorMessage = message
                };
            }

            public static ParseCsvResult Ok(List<SourceDataPoint> rows)
            {
                return new ParseCsvResult
                {
                    HasErrors = false,
                    Rows = rows
                };
            }
        }
    }
}
