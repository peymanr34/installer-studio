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
            ViewModel = new EditSetupViewModel(App.Context);
        }

        public EditSetupViewModel ViewModel { get; private set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var setup = (SetupViewModel)e.Parameter;

            ViewModel.Load(setup.Id);
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
