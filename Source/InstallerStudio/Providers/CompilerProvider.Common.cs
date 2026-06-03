using System;
using System.IO;
using InstallerStudio.Models;
using Microsoft.Win32;

namespace InstallerStudio.Providers
{
    public static partial class CompilerProvider
    {
        private static Compiler GetCompilerFromRegistryKey(RegistryKey key, string executableName)
        {
            if (key?.GetValue("InstallLocation") is string path)
            {
                var compiler = new Compiler
                {
                    Name = key.GetValue("DisplayName") as string,
                    Path = Path.Join(path, executableName),
                };

                var displayVersion = key.GetValue("DisplayVersion") as string;
                int index = displayVersion.IndexOf('-');

                if (index >= 0)
                {
                    displayVersion = displayVersion[..index];
                }

                if (Version.TryParse(displayVersion, out var version))
                {
                    compiler.Version = version;
                }

                return compiler;
            }

            return null;
        }

        private static string GetCompilerRegistryKeyPath(string relativePath, bool useWow6432 = false)
        {
            var baseKeyPath = "SOFTWARE\\";

            if (useWow6432)
            {
                baseKeyPath += "Wow6432Node\\";
            }

            return $"{baseKeyPath}Microsoft\\Windows\\CurrentVersion\\Uninstall\\{relativePath}";
        }
    }
}
