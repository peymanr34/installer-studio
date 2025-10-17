using Windows.Win32;

namespace InstallerStudio.Providers
{
    public static partial class SetupDetector
    {
        public static bool IsMsi(string filePath)
        {
            // https://learn.microsoft.com/en-us/windows/win32/api/msi/nf-msi-msiverifypackagew
            return PInvoke.MsiVerifyPackage(filePath) == 0;
        }
    }
}
