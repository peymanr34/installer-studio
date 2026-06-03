using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using InstallerStudio.Models;

namespace InstallerStudio
{
    public static class InnoExtensions
    {
        public static string AsString(this InnoFile item)
        {
            var result = $"Source: \"{item.Source}\"; DestDir: \"{item.DestDir}\"";

            if (item.Flags.Count != 0)
            {
                result += $"; Flags: {string.Join(" ", item.Flags)}";
            }

            if (item.Components.Count != 0)
            {
                result += $"; Components: {string.Join(" ", item.Components)}";
            }

            if (item.Checks.Count != 0)
            {
                result += $"; Check: {string.Join(" and ", item.Checks)}";
            }

            return result;
        }

        public static string AsString(this InnoLanguage item)
        {
            return $"Name: \"{item.Name}\"; MessagesFile: \"{item.MessagesFile}\"";
        }

        public static string AsString(this InnoType item)
        {
            var result = $"Name: \"{item.Name}\"; Description: \"{item.Description}\"";

            if (item.Flags.Count != 0)
            {
                result += $"; Flags: {string.Join(" ", item.Flags)}";
            }

            return result;
        }

        public static string AsString(this InnoComponent item)
        {
            var result = $"Name: \"{item.Name}\"; Description: \"{item.Description}\"";

            if (item.Types.Count != 0)
            {
                result += $"; Types: {string.Join(" ", item.Types)}";
            }

            if (item.Checks.Count != 0)
            {
                result += $"; Check: {string.Join(" and ", item.Checks)}";
            }

            return result;
        }

        public static string AsString(this InnoRun item)
        {
            var result = $"Filename: \"{item.FileName}\"; Parameters: \"{item.Parameters}\"";

            if (item.Flags.Count != 0)
            {
                result += $"; Flags: {string.Join(" ", item.Flags)}";
            }

            result += $"; StatusMsg: \"{item.StatusMsg}\"";

            if (item.Components.Count != 0)
            {
                result += $"; Components: {string.Join(" ", item.Components)}";
            }

            return result;
        }

        public static string AsString(this InnoScript item)
        {
            var builder = new StringBuilder();

            foreach (var line in item.AsLines())
            {
                builder.AppendLine(line);
            }

            return builder.ToString();
        }

        private static ReadOnlyCollection<string> AsLines(this InnoScript item)
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
