using System;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.ApplicationInstallationAndServicing;

namespace InstallerStudio.Providers
{
    public static partial class SetupProvider
    {
        public static bool IsMsi(string filePath)
        {
            // https://learn.microsoft.com/en-us/windows/win32/api/msi/nf-msi-msiverifypackagew
            return PInvoke.MsiVerifyPackage(filePath) == 0;
        }

        public static unsafe SetupInfo GetMsiInfo(string filePath)
        {
            SetupInfo info = null;
            MSIHANDLE hDatabase = default;

            if (PInvoke.MsiOpenDatabase(filePath, null, ref hDatabase) == 0)
            {
                info = new SetupInfo
                {
                    Name = GetPropertyValue(hDatabase, "ProductName"),
                    Version = GetPropertyValue(hDatabase, "ProductVersion"),
                };

                _ = PInvoke.MsiCloseHandle(hDatabase);
            }

            return info;
        }

        private static unsafe string GetPropertyValue(MSIHANDLE hDatabase, string propertyName)
        {
            MSIHANDLE hView = default;

            fixed (char* pQuery = $"SELECT Value FROM Property WHERE Property = '{propertyName}'")
            {
                if (PInvoke.MsiDatabaseOpenView(hDatabase, new PCWSTR(pQuery), &hView) != 0)
                {
                    return null;
                }
            }

            MSIHANDLE hRecord = default;

            if (PInvoke.MsiViewExecute(hView, hRecord) != 0)
            {
                return null;
            }

            if (PInvoke.MsiViewFetch(hView, &hRecord) != 0)
            {
                return null;
            }

            uint cchValueBuf = 0;

            fixed (char* pEmpty = "")
            {
                // Get the size of the buffer by passing in an empty string.
                var result = PInvoke.MsiRecordGetString(hRecord, 1, new PWSTR(pEmpty), &cchValueBuf);

                // The function returns ERROR_MORE_DATA (234) and pcchValueBuf contains the required
                // buffer size in TCHARs, not including the terminating null character.
                if (result == 234)
                {
                    ++cchValueBuf;
                    Span<char> buffer = new char[cchValueBuf];

                    fixed (char* pBuffer = buffer)
                    {
                        if (PInvoke.MsiRecordGetString(hRecord, 1, new PWSTR(pBuffer), &cchValueBuf) == 0)
                        {
                            return buffer.ToString().TrimEnd('\0');
                        }
                    }
                }
            }

            return null;
        }
    }
}
