using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using data.AssetManager;
namespace desktop.app.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly AssetManager _assetManager;
    [ObservableProperty]
    private ObservableCollection<ProductionUnit> _units;
    public MainViewModel()
    {
        _assetManager = new AssetManager();
        _units = new ObservableCollection<ProductionUnit>(_assetManager.GetProductionUnits());
    }
   [RelayCommand]
    private void AddProductionUnit()
        {
            
        }
           [RelayCommand]
    private void DeleteProductionUnit()
        {
            
        }
   [RelayCommand]
    private void UpdateProductionUnit()
        {
            
        }
}
}
