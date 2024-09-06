using System;
using InstallerStudio.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace InstallerStudio.Pages
{
    public sealed partial class EditPage : Page
    {
        public EditPage()
        {
            InitializeComponent();
        }

        public EditSetupViewModel ViewModel { get; private set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var setup = e.Parameter as SetupViewModel;

            ViewModel = new EditSetupViewModel(App.Context, setup.Id);
            ViewModel.Load();

            ViewModel.Saved += ViewModel_Saved;

            base.OnNavigatedTo(e);
        }

        private void ViewModel_Saved(object sender, EventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }
    }
}
