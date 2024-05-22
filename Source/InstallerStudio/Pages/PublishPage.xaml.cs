using InstallerStudio.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace InstallerStudio.Pages
{
    public sealed partial class PublishPage : Page
    {
        public PublishPage()
        {
            InitializeComponent();
        }

        public PublishViewModel ViewModel { get; private set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var project = e.Parameter as ProjectViewModel;

            ViewModel = new PublishViewModel(App.Context, project.Id);

            base.OnNavigatedTo(e);
        }

        private void RichTextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            scrollViewer.ScrollToVerticalOffset(scrollViewer.ScrollableHeight);
        }
    }
}
