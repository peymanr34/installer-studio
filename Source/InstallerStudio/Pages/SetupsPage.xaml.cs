using System;
using System.Linq;
using InstallerStudio.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.ApplicationModel.DataTransfer;
using Windows.Services.Store;
using Windows.Storage;
using WinRT.Interop;

namespace InstallerStudio.Pages
{
    public sealed partial class SetupsPage : Page
    {
        public SetupsPage()
        {
            InitializeComponent();
        }

        public SetupsViewModel ViewModel { get; private set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var project = e.Parameter as ProjectViewModel;

            ViewModel = new SetupsViewModel(App.Context, project.Id);
            ViewModel.Load();

            base.OnNavigatedTo(e);
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                ViewModel.Load(sender.Text);
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedItem is null)
            {
                return;
            }

            Frame.Navigate(typeof(EditPage), ViewModel.SelectedItem);
        }

        private async void About_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "About",
                CloseButtonText = "OK",
                Content = new AboutPage(),
                XamlRoot = Content.XamlRoot,
            };

            await dialog.ShowAsync();
        }

        private async void Rate_Click(object sender, RoutedEventArgs e)
        {
            var context = StoreContext.GetDefault();

            var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
            InitializeWithWindow.Initialize(context, hwnd);

            await context.RequestRateAndReviewAppAsync();
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Link;
        }

        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                await ViewModel.Create(items.Cast<StorageFile>());
            }
        }
    }
}
