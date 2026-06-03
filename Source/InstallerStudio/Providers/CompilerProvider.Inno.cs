using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using InstallerStudio.Models;
using Microsoft.Win32;

namespace InstallerStudio.Providers
{
    public static partial class CompilerProvider
    {
        private static ReadOnlyCollection<Compiler> GetInstalledInnoSetupCompilers()
        {
            var compilers = new List<Compiler>();

            foreach (var keyPath in GetInnoSetupRegistryKeyPaths())
            {
                using var key = Registry.LocalMachine.OpenSubKey(keyPath)
                    ?? Registry.CurrentUser.OpenSubKey(keyPath);

                var compiler = GetCompilerFromRegistryKey(key, "ISCC.exe");

                if (compiler is not null)
                {
                    compilers.Add(compiler);
                }
            }

            return compilers.AsReadOnly();
        }

        private static IEnumerable<string> GetInnoSetupRegistryKeyPaths()
        {
            // Inno Setup 7 (64-bit) on 64-bit Windows or Inno Setup 7 (32-bit) on 32-bit Windows.
            yield return GetCompilerRegistryKeyPath("Inno Setup 7_is1");
            // Inno Setup 7 (32-bit) on 64-bit Windows.
            yield return GetCompilerRegistryKeyPath("Inno Setup 7_is1", true);
            // Inno Setup 6 is 32-bit only.
            yield return GetCompilerRegistryKeyPath("Inno Setup 6_is1", Environment.Is64BitOperatingSystem);
        }
    }
}
