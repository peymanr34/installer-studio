using InstallerStudio.Pages;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.Graphics;
using WinRT.Interop;

namespace InstallerStudio
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeSystemBackdrop();

            Title = "Installer Studio";
            ExtendsContentIntoTitleBar = true;

            var size = GetScaledWindowSize(550, 950);

            AppWindow.Resize(size);
            AppWindow.SetIcon("Assets\\icon.ico");

            contentFrame.Navigate(typeof(StartPage));
        }

        private SizeInt32 GetScaledWindowSize(int height, int width)
        {
            var hwnd = WindowNative.GetWindowHandle(this);
            var dpi = NativeMethods.GetDpiForWindow(hwnd);

            var scale = dpi / 96d;

            return new SizeInt32
            {
                Height = (int)(height * scale),
                Width = (int)(width * scale),
            };
        }

        private void InitializeSystemBackdrop()
        {
            if (MicaController.IsSupported())
            {
                SystemBackdrop = new MicaBackdrop()
                {
                    Kind = MicaKind.BaseAlt,
                };
            }
            else if (DesktopAcrylicController.IsSupported())
            {
                SystemBackdrop = new DesktopAcrylicBackdrop();
            }
        }
    }
}
