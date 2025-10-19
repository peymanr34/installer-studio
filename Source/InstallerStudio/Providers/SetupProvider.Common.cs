using System;
using System.Linq;

namespace InstallerStudio.Providers
{
    public static partial class SetupProvider
    {
        public static bool IsX86Setup(string fileName)
        {
            return IsArchCore(fileName, ["x86", "32bit"]);
        }

        public static bool IsX64Setup(string fileName)
        {
            return IsArchCore(fileName, ["x64", "64bit", "amd64"]);
        }

        public static bool IsArm64Setup(string fileName)
        {
            return IsArchCore(fileName, ["arm64", "aarch64"]);
        }

        private static bool IsArchCore(string fileName, string[] allowed)
        {
            var normalized = GetNormalizedString(fileName, string.Empty);
            return allowed.Any(x => normalized.Contains(x, StringComparison.OrdinalIgnoreCase));
        }

        private static string GetNormalizedString(string str, string separator)
        {
            var items = new[] { " ", "(", ")", "_", "-", "..", "–", "[", "]", "{", "}" };

            foreach (var item in items.Where(item => str.Contains(item, StringComparison.Ordinal)))
            {
                str = str.Replace(item, separator, StringComparison.Ordinal);
            }

            return str;
        }
    }
}
