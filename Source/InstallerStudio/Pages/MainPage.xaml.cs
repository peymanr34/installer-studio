using System;
using System.Linq;
using InstallerStudio.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;

namespace InstallerStudio.Pages
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        public ProjectViewModel ViewModel { get; private set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel = e.Parameter as ProjectViewModel;
            base.OnNavigatedTo(e);
        }

        private void NavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            contentFrame.Navigated += ContentFrame_Navigated;
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            var allowed = new[]
            {
                typeof(EditPage),
            };

            navView.IsBackEnabled = contentFrame.CanGoBack && allowed.Contains(e.SourcePageType);
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var page = args.SelectedItemContainer.Tag switch
            {
                "Setups" => typeof(SetupsPage),
                "Details" => typeof(DetailsPage),
                "Publish" => typeof(PublishPage),
                _ => null,
            };

            Navigate(page, ViewModel);
        }

        private void NavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (contentFrame.CanGoBack)
            {
                contentFrame.GoBack();
            }
        }

        private void Navigate(Type page, object parameter)
        {
            // Get the page type before navigation so you can prevent duplicate entries in the back-stack.
            var previousPage = contentFrame.CurrentSourcePageType;

            // Only navigate if the selected page isn't currently loaded.
            if (page is not null && !Equals(previousPage, page))
            {
                contentFrame.Navigate(page, parameter, new DrillInNavigationTransitionInfo());
            }
        }
    }
}
