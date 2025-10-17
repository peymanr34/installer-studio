using System;
using System.IO;
using System.Linq;

namespace InstallerStudio.Providers
{
    public static class SetupProvider
    {
        public enum SetupType
        {
            Unknown,
            InnoSetup,
            Nullsoft,
            Burn,
            Msi,
        }

        public static SetupType GetSetupType(string filePath)
        {
            var extension = Path.GetExtension(filePath);

            if (extension.Equals(".msi", StringComparison.OrdinalIgnoreCase) &&
                SetupDetector.IsMsi(filePath))
            {
                return SetupType.Msi;
            }

            if (extension.Equals(".exe", StringComparison.OrdinalIgnoreCase))
            {
                if (SetupDetector.IsNullsoft(filePath))
                {
                    return SetupType.Nullsoft;
                }
                else if (SetupDetector.IsInnoSetup(filePath))
                {
                    return SetupType.InnoSetup;
                }
                else if (SetupDetector.IsBurn(filePath))
                {
                    return SetupType.Burn;
                }
            }

            return SetupType.Unknown;
        }

        public static string GetSilentSwitch(SetupType type)
        {
            return type switch
            {
                SetupType.InnoSetup => "/VERYSILENT",
                SetupType.Nullsoft => "/S",
                SetupType.Burn => "/quiet",
                SetupType.Msi => "/quiet",
                _ => null,
            };
        }

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
