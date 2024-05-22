using InstallerStudio.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace InstallerStudio.Pages
{
    public sealed partial class StartPage : Page
    {
        public StartPage()
        {
            InitializeComponent();

            ViewModel = new StartViewModel(App.Context);
            ViewModel.Load();
        }

        public StartViewModel ViewModel { get; private set; }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedItem is null)
            {
                return;
            }

            Frame.Navigate(typeof(MainPage), ViewModel.SelectedItem);
        }

        private void ProjectNameTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter &&
                ViewModel.CreateCommand.CanExecute(null))
            {
                ViewModel.CreateCommand.Execute(null);
            }
        }
    }
}
