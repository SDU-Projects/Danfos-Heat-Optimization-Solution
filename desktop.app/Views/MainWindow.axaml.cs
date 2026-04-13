using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using desktop.app.ViewModels;
using System.Collections.Generic;

namespace desktop.app.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext= new MainWindowViewModel();
        }

        private async void UploadCsvButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel viewModel)
            {
                return;
            }

            TopLevel? topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null)
            {
                return;
            }

            IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = false,
                Title = "Select CSV dataset",
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("CSV files")
                    {
                        Patterns = new[] { "*.csv" }
                    }
                }
            });

            if (files.Count == 0)
            {
                return;
            }

            string? localPath = files[0].TryGetLocalPath();
            if (string.IsNullOrWhiteSpace(localPath))
            {
                return;
            }

            await viewModel.ProcessCsvUploadAsync(localPath);
        }
    }
}