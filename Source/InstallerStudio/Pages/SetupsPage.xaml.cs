using System;
using InstallerStudio.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

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
    }
}
