using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.LibraryLoader;

namespace InstallerStudio.Providers
{
    public static class InnoDetector
    {
        private const string _tableResId = "11111";

        public static bool IsInnoSetup(string filePath)
        {
            // Resource name of '11111' indicates an inno setup installer:
            // https://github.com/microsoft/winget-pkgs/blob/master/Tools/Modules/YamlCreate/YamlCreate.InstallerDetection/YamlCreate.InstallerDetection.psm1#L314
            // https://github.com/jrsoftware/issrc/blob/main/Projects/Src/Shared.Struct.pas#L426
            return GetResourceNames(filePath).Contains(_tableResId);
        }

        private static List<string> GetResourceNames(string filePath)
        {
            var items = new List<string>();

            BOOL EnumResources(HMODULE hModule, PCWSTR lpType, PWSTR lpName, nint lParam)
            {
                items.Add(GetResourceName(lpName));
                return true;
            }

            var handle = PInvoke.LoadLibraryEx(filePath, LOAD_LIBRARY_FLAGS.LOAD_LIBRARY_AS_DATAFILE);

            // RT_RCDATA = 10
            // ResourceTypes: https://learn.microsoft.com/en-us/windows/win32/menurc/resource-types
            // EnumResourceNamesW: https://learn.microsoft.com/en-us/windows/win32/api/libloaderapi/nf-libloaderapi-enumresourcenamesw
            if (!PInvoke.EnumResourceNames(handle, "#10", new ENUMRESNAMEPROCW(EnumResources), IntPtr.Zero))
            {
                Debug.WriteLine($"Win32Error: {Marshal.GetLastWin32Error()}");
            }

            handle.Close();
            return items;
        }

        private static unsafe string GetResourceName(PWSTR lpName)
        {
            var ptr = new IntPtr(lpName.Value);

            // IS_INTRESOURCE
            // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-is_intresource
            // https://github.com/terrafx/terrafx.interop.windows/blob/main/sources/Interop/Windows/Windows/um/WinUser/Windows.Manual.cs#L32
            if ((ptr >> 16) == 0)
            {
                return ptr.ToString();
            }

            // https://pinvoke.net/default.aspx/kernel32.enumresourcenames
            return Marshal.PtrToStringUni(ptr);
        }
    }
}
