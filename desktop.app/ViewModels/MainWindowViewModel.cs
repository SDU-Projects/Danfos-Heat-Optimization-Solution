using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using data.Entities;
using data.Models.Base;
using data.Services;

namespace desktop.app.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly AssetService _assetService;

        public MainWindowViewModel()
        {
            _assetService = new AssetService();
            ProductionUnits = new ObservableCollection<ProductionUnit>();
            LoadProductionUnitsAsync();
            SelectedProductionUnitType = ProductionUnitTypes.First();

            ShowCatalog();
        }

        [ObservableProperty]
        public ObservableCollection<ProductionUnit> productionUnits;

        [ObservableProperty]
        public ProductionUnit? selectedUnit;

        [ObservableProperty]
        private bool isCatalogVisible = true;

        [ObservableProperty]
        private bool isCreateVisible;

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
        private string selectedProductionUnitType;

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
            IsUpdateVisible = false;
            IsDeleteVisible = false;
        }

        [RelayCommand]
        public void ShowCreate()
        {
            IsCatalogVisible = false;
            IsCreateVisible = true;
            IsUpdateVisible = false;
            IsDeleteVisible = false;
            ClearForm();
        }
        [RelayCommand]
        private async Task AddProductionUnitAsync()
        {
            if (string.IsNullOrWhiteSpace(NewUnitName) || string.IsNullOrWhiteSpace(NewUnitImageUrl))
                return;

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

                var created = await _assetService.CreateAssetAsync(newAsset);
                if (created != null)
                {
                    ProductionUnits.Add(created);
                    OnPropertyChanged(nameof(FilteredProductionUnits));
                    ShowCatalog();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creando ProductionUnit: {ex.Message}");
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
        public void RefreshCatalog()
        {
            LoadProductionUnitsAsync();
        }

        private void ClearForm()
        {
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
    }
}
