using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using InstallerStudio.Models;
using Windows.Storage;

namespace InstallerStudio.Providers
{
    public static partial class CompilerProvider
    {
        public static ReadOnlyCollection<CompilerInfo> GetSupportedCompilers()
        {
            var items = new List<CompilerInfo>
            {
                new()
                {
                    Name = "Inno Setup",
                    SupportedVersion = new Version(6, 3, 0),
                    CompilerType = CompilerType.InnoSetup,
                    ScriptFileExtension = ".iss",
                    DownloadUrl = "https://jrsoftware.org/isdl.php",
                },
                new()
                {
                    Name = "Nullsoft Scriptable Install System",
                    SupportedVersion = new Version(3, 12),
                    CompilerType = CompilerType.Nullsoft,
                    ScriptFileExtension = ".nsi",
                    DownloadUrl = "https://nsis.sourceforge.io/Download",
                },
            };

            return items.AsReadOnly();
        }

        public static IEnumerable<Compiler> GetInstalledCompilers(CompilerType compilerType) => compilerType switch
        {
            CompilerType.InnoSetup => GetInstalledInnoSetupCompilers(),
            CompilerType.Nullsoft => GetInstalledNullsoftCompilers(),
            _ => throw new NotImplementedException(),
        };

        public static ReadOnlyCollection<string> GetArgumentsForScript(StorageFile script, StorageFile file, StorageFolder folder, CompilerType compilerType)
        {
            var items = new List<string>();

            if (compilerType == CompilerType.InnoSetup)
            {
                items.Add(script.Path); // Script file name.
                items.Add($"/O{folder.Path}"); // Output files to specified path.
                items.Add($"/F{file.DisplayName}"); // Specifies an output filename.
            }
            else if (compilerType == CompilerType.Nullsoft)
            {
                items.Add("/V3"); // Set the verbosity of the output.
                items.Add($"/DOutFile={file.Path}"); // Set the output filename variable.
                items.Add(script.Path); // Should always be the last argument.
            }
            else
            {
                throw new NotImplementedException();
            }

            return items.AsReadOnly();
        }
    }
}
