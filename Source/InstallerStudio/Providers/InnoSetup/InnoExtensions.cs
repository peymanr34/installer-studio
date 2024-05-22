using InstallerStudio.Providers.InnoSetup.Models;

namespace InstallerStudio.Providers.InnoSetup
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
    }
}
