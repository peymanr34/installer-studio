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
            ViewModel = new EditProjectViewModel(App.Context);
        }

        public EditProjectViewModel ViewModel { get; private set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var project = (ProjectViewModel)e.Parameter;

            ViewModel.Load(project.Id);

            base.OnNavigatedTo(e);
        }
    }
}
