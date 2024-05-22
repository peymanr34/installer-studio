using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using InstallerStudio.Data.Models;
using InstallerStudio.Providers.InnoSetup.Models;

namespace InstallerStudio.Providers
{
    public static class InnoCreator
    {
        public static InnoScript CreateSetupScript(Project project)
        {
            var script = new InnoScript();

            // Add the basics.
            script.Setup.Add("AppId", $"{{{{{project.UniqueId}}}");
            script.Setup.Add("AppName", project.Name);
            script.Setup.Add("AppVersion", project.Version);
            script.Setup.Add("AppVerName", $"{project.Name} (v{project.Version})");

            if (!string.IsNullOrEmpty(project.Publisher))
            {
                script.Setup.Add("AppPublisher", project.Publisher);
            }

            if (!string.IsNullOrEmpty(project.Website))
            {
                script.Setup.Add("AppPublisherURL", project.Website);
            }

            if (project.SetupType == SetupType.Internal)
            {
                script.Setup.Add("DiskSpanning", "yes");
            }

            script.Setup.Add("CreateAppDir", "no");
            script.Setup.Add("Compression", "none");
            script.Setup.Add("Uninstallable", "no");
            script.Setup.Add("WizardStyle", "modern");
            script.Setup.Add("DisableReadyMemo", "yes");
            script.Setup.Add("ShowLanguageDialog", "no");

            // Add the languages.
            var english = new InnoLanguage
            {
                Name = "english",
                MessagesFile = "compiler:Default.isl",
            };

            script.Languages.Add(english);

            // Add the types.
            var full = new InnoType
            {
                Name = "full",
                Description = "Full installation",
            };

            script.Types.Add(full);

            var custom = new InnoType
            {
                Name = "custom",
                Description = "Custom installation",
            };

            custom.Flags.Add("iscustom");
            script.Types.Add(custom);

            // Specify the destination.
            var location = project.SetupType switch
            {
                SetupType.Internal => "{tmp}",
                SetupType.External => "{src}\\Files",
                _ => throw new NotImplementedException(),
            };

            foreach (var item in project.Setups)
            {
                var component = new InnoComponent
                {
                    Name = item.GetIdentifier(),
                    Description = item.Name ?? item.Description,
                };

                component.Types.Add(full.Name);
                component.Types.Add(custom.Name);
                component.Checks.AddRange(GetConditions(item));

                script.Components.Add(component);

                if (project.SetupType == SetupType.Internal)
                {
                    // Add the main file.
                    script.Files.Add(CreateFile(item, item.FilePath, location));

                    // Add the extra files.
                    var extras = item.Additionals
                        .Select(x => CreateFileOrFolder(item, x.IsDirectory, x.Path, location));

                    script.Files.AddRange(extras);
                }

                script.Runs.Add(CreateRun(item, location));
            }

            return script;
        }

        private static InnoRun CreateRun(Setup setup, string location)
        {
            var item = new InnoRun
            {
                FileName = $"{location}\\{setup.GetIdentifier()}\\{Path.GetFileName(setup.FilePath)}",
                Parameters = setup.Arguments,
                StatusMsg = $"Installing {setup.Name}...",
            };

            item.Checks.AddRange(GetConditions(setup));
            item.Flags.Add("waituntilterminated");
            item.Components.Add(setup.GetIdentifier());

            var extension = Path.GetExtension(setup.FilePath);

            if (extension.Equals(".msi", StringComparison.OrdinalIgnoreCase))
            {
                item.Flags.Add("shellexec");
            }

            return item;
        }

        private static InnoFile CreateFileOrFolder(Setup setup, bool folder, string source, string destination)
        {
            if (folder)
            {
                return CreateFolder(setup, source, destination);
            }

            return CreateFile(setup, source, destination);
        }

        private static InnoFile CreateFile(Setup setup, string source, string destination)
        {
            var item = new InnoFile
            {
                Source = source,
                DestDir = $"{destination}\\{setup.GetIdentifier()}",
            };

            item.Flags.Add("ignoreversion");
            item.Components.Add(setup.GetIdentifier());
            item.Checks.AddRange(GetConditions(setup));

            return item;
        }

        private static InnoFile CreateFolder(Setup setup, string source, string destination)
        {
            var info = new DirectoryInfo(source);

            var item = new InnoFile
            {
                Source = $"{source}\\*",
                DestDir = $"{destination}\\{setup.GetIdentifier()}\\{info.Name}",
            };

            item.Flags.Add("ignoreversion");
            item.Flags.Add("recursesubdirs");
            item.Flags.Add("createallsubdirs");

            item.Components.Add(setup.GetIdentifier());
            item.Checks.AddRange(GetConditions(setup));

            return item;
        }

        private static ReadOnlyCollection<string> GetConditions(Setup setup)
        {
            var items = new List<string>();

            if (setup.IsX86)
            {
                items.Add("IsX86");
            }

            if (setup.IsX64)
            {
                items.Add("IsX64");
            }

            if (setup.IsArm64)
            {
                items.Add("IsARM64");
            }

            return items.AsReadOnly();
        }
    }
}
