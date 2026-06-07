using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using InstallerStudio.Models;
using Microsoft.Win32;

namespace InstallerStudio.Providers
{
    public static partial class CompilerProvider
    {
        private static ReadOnlyCollection<Compiler> GetInstalledNullsoftCompilers()
        {
            var compilers = new List<Compiler>();

            foreach (var keyPath in GetNullsoftRegistryKeyPaths())
            {
                using var key = Registry.LocalMachine.OpenSubKey(keyPath);
                var compiler = GetCompilerFromRegistryKey(key, "makensis.exe");

                if (compiler is not null)
                {
                    compilers.Add(compiler);
                }
            }

            return compilers.AsReadOnly();
        }

        private static IEnumerable<string> GetNullsoftRegistryKeyPaths()
        {
            yield return GetCompilerRegistryKeyPath("NSIS", Environment.Is64BitOperatingSystem);
        }
    }
}
