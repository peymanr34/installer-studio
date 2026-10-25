using System.ComponentModel;
using InstallerStudio.Providers;
using InstallerStudio.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Win32.UI.Shell;

namespace InstallerStudio.Pages
{
    public sealed partial class PublishPage : Page
    {
        private readonly TaskbarManager _taskbarManager;

        public PublishPage()
        {
            InitializeComponent();

            _taskbarManager = TaskbarManager.Create(App.MainWindow);

            ViewModel = new PublishViewModel(App.Context);
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        public PublishViewModel ViewModel { get; private set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var project = (ProjectViewModel)e.Parameter;

            ViewModel.Load(project.Id);

            base.OnNavigatedTo(e);
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.IsExecuting))
            {
                var state = ViewModel.IsExecuting ? TBPFLAG.TBPF_INDETERMINATE : TBPFLAG.TBPF_NOPROGRESS;
                _taskbarManager?.SetProgressState(state);
            }
        }

        private void RichTextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            scrollViewer.ScrollToVerticalOffset(scrollViewer.ScrollableHeight);
        }
    }
}
