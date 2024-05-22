using Windows.Win32;
using Windows.Win32.Foundation;

namespace InstallerStudio
{
    internal static partial class NativeMethods
    {
        public static uint GetDpiForWindow(nint hwnd)
        {
            return PInvoke.GetDpiForWindow(new HWND(hwnd));
        }
    }
}
