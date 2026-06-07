using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using InstallerStudio.Data.Models;

namespace InstallerStudio.Providers
{
    public static partial class SetupCreator
    {
        public static string CreateNullsoftScript(Project project)
        {
            var builder = new StringBuilder();

            builder.AppendLine("!include MUI2.nsh");
            builder.AppendLine("!include LogicLib.nsh");
            builder.AppendLine("!include Sections.nsh");
            builder.AppendLine("!include x64.nsh");
            builder.AppendLine();

            builder.AppendLine("!define MUI_ABORTWARNING");
            builder.AppendLine("!define MUI_COMPONENTSPAGE_NODESC");
            builder.AppendLine();

            builder.AppendLine("!define MUI_ICON \"${NSISDIR}\\Contrib\\Graphics\\Icons\\nsis3-install-alt.ico\"");
            builder.AppendLine("!define MUI_UNICON \"${NSISDIR}\\Contrib\\Graphics\\Icons\\nsis3-uninstall.ico\"");
            builder.AppendLine();

            builder.AppendLine("!define MUI_HEADERIMAGE");
            builder.AppendLine("!define MUI_HEADERIMAGE_RIGHT");
            builder.AppendLine("!define MUI_HEADERIMAGE_BITMAP \"${NSISDIR}\\Contrib\\Graphics\\Header\\nsis3-metro-right.bmp\"");
            builder.AppendLine("!define MUI_WELCOMEFINISHPAGE_BITMAP \"${NSISDIR}\\Contrib\\Graphics\\Wizard\\nsis3-metro.bmp\"");
            builder.AppendLine();

            builder.AppendLine("!insertmacro MUI_PAGE_COMPONENTS");
            builder.AppendLine("!insertmacro MUI_PAGE_INSTFILES");
            builder.AppendLine("!insertmacro MUI_PAGE_FINISH");
            builder.AppendLine("!insertmacro MUI_LANGUAGE \"English\"");
            builder.AppendLine();

            builder.AppendLine($"Name \"{project.Name} (v{project.Version})\"");
            builder.AppendLine($"BrandingText \"{project.Name} (v{project.Version})\"");
            builder.AppendLine();

            builder.AppendLine("OutFile \"${OutFile}\"");
            builder.AppendLine("ShowInstDetails show");
            builder.AppendLine("SetCompress off");

            // Specify the destination.
            var location = project.SetupType switch
            {
                SetupType.Internal => "$PLUGINSDIR",
                SetupType.External => "$EXEDIR\\Files",
                _ => throw new NotImplementedException(),
            };

            foreach (var item in project.Setups)
            {
                builder.AppendLine();
                builder.AppendLine($"Section \"{item.Name ?? item.Description}\" {item.GetIdentifier()}");
                builder.AppendLine($"  SetOutPath \"{location}\\{item.GetIdentifier()}\"");
                builder.AppendLine($"  File \"{item.FilePath}\"");
                builder.AppendLine($"  DetailPrint \"Installing {item.Name}...\"");
                builder.AppendLine($"  {CreateNullsoftExecute(item, location)}");
                builder.AppendLine("SectionEnd");
            }

            builder.AppendLine();
            builder.AppendLine("Function .onInit");
            builder.AppendLine("  InitPluginsDir");

            foreach (var item in project.Setups)
            {
                var conditions = GetNullsoftConditions(item);

                if (conditions.Count != 0)
                {
                    builder.Append("  ${IfNot} ");
                    builder.AppendLine(string.Join("\n  ${AndIfNot} ", conditions));
                    builder.AppendLine($"    !insertmacro RemoveSection ${{{item.GetIdentifier()}}}");
                    builder.AppendLine("  ${EndIf}");
                }
            }

            builder.AppendLine("FunctionEnd");
            return builder.ToString();
        }

        private static string CreateNullsoftExecute(Setup setup, string location)
        {
            var extension = Path.GetExtension(setup.FilePath);
            var destination = $"{location}\\{setup.GetIdentifier()}\\{Path.GetFileName(setup.FilePath)}";

            string item = string.Empty;

            if (extension.Equals(".msi", StringComparison.OrdinalIgnoreCase))
            {
                item += $"\"msiexec\" /i ";
            }

            item += $"\"{destination}\"";

            if (!string.IsNullOrEmpty(setup.Arguments))
            {
                item += $" {setup.Arguments}";
            }

            return $"ExecWait '{item}'";
        }

        private static ReadOnlyCollection<string> GetNullsoftConditions(Setup setup)
        {
            var items = new List<string>();

            if (setup.IsX86)
            {
                items.Add("${IsNativeIA32}");
            }

            if (setup.IsX64)
            {
                items.Add("${IsNativeAMD64}");
            }

            if (setup.IsArm64)
            {
                items.Add("${IsNativeARM64}");
            }

            return items.AsReadOnly();
        }
    }
}
