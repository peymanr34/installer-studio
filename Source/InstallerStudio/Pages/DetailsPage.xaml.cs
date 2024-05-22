using InstallerStudio.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace InstallerStudio.Pages
{
    public sealed partial class DetailsPage : Page
    {
        public DetailsPage()
        {
            InitializeComponent();
        }

        public EditProjectViewModel ViewModel { get; private set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var project = e.Parameter as ProjectViewModel;

            ViewModel = new EditProjectViewModel(App.Context, project.Id);
            ViewModel.Load();

            base.OnNavigatedTo(e);
        }
    }
}
