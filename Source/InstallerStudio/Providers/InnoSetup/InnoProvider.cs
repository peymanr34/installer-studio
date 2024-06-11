using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using InstallerStudio.Providers.InnoSetup.Models;
using Microsoft.Win32;

namespace InstallerStudio.Providers.InnoSetup
{
    public static class InnoProvider
    {
        public static InnoCompiler GetCompiler()
        {
            var baseKeyPath = "SOFTWARE\\";

            if (Environment.Is64BitOperatingSystem)
            {
                baseKeyPath += "Wow6432Node\\";
            }

            var keyPath = $@"{baseKeyPath}Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 6_is1";

            using var key = Registry.LocalMachine.OpenSubKey(keyPath);

            if (key?.GetValue("InstallLocation") is string path &&
                key?.GetValue("DisplayVersion") is string version)
            {
                return new InnoCompiler
                {
                    Path = Path.Join(path, "ISCC.exe"),
                    Version = Version.Parse(version),
                };
            }

            return null;
        }

        public static ReadOnlyCollection<string> GetSetupScript(InnoScript item)
        {
            var items = new List<string>();

            if (item.Setup.Count != 0)
            {
                items.Add("[Setup]");
                items.AddRange(item.Setup.Select(setup => $"{setup.Key}={setup.Value}"));
            }

            if (item.Languages.Count != 0)
            {
                items.Add("[Languages]");
                items.AddRange(item.Languages.Select(x => x.AsString()));
            }

            if (item.Types.Count != 0)
            {
                items.Add("[Types]");
                items.AddRange(item.Types.Select(x => x.AsString()));
            }

            if (item.Components.Count != 0)
            {
                items.Add("[Components]");
                items.AddRange(item.Components.Select(x => x.AsString()));
            }

            if (item.Files.Count != 0)
            {
                items.Add("[Files]");
                items.AddRange(item.Files.Select(x => x.AsString()));
            }

            if (item.Runs.Count != 0)
            {
                items.Add("[Run]");
                items.AddRange(item.Runs.Select(x => x.AsString()));
            }

            return items.AsReadOnly();
        }
    }
}
