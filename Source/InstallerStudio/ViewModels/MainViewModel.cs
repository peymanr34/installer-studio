using Microsoft.UI.Xaml.Controls;
using MvvmGen;

namespace InstallerStudio.ViewModels
{
    [ViewModel]
    public partial class MainViewModel
    {
        [Property]
        private ProjectViewModel _project;

        [Property]
        private NavigationViewItem _selectedItem;
    }
}
