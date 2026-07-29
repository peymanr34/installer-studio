using System;
using System.Diagnostics;
using System.IO;

namespace InstallerStudio.Providers
{
    public static partial class SetupProvider
    {
        public enum SetupType
        {
            Unknown,
            InnoSetup,
            Nullsoft,
            Burn,
            Msi,
        }

        public class SetupInfo
        {
            public string Name { get; set; }

            public string Version { get; set; }

            public string Description { get; set; }
        }

        public static SetupInfo GetSetupInfo(string filePath, SetupType setupType)
        {
            if (setupType == SetupType.Msi)
            {
                return GetMsiInfo(filePath);
            }

            var info = FileVersionInfo.GetVersionInfo(filePath);

            return new SetupInfo
            {
                Name = info.ProductName,
                Version = info.ProductVersion,
                Description = info.FileDescription,
            };
        }

        public static SetupType GetSetupType(string filePath)
        {
            var extension = Path.GetExtension(filePath);

            if (extension.Equals(".msi", StringComparison.OrdinalIgnoreCase) &&
                IsMsi(filePath))
            {
                return SetupType.Msi;
            }

            if (extension.Equals(".exe", StringComparison.OrdinalIgnoreCase))
            {
                if (IsNullsoft(filePath))
                {
                    return SetupType.Nullsoft;
                }
                else if (IsInnoSetup(filePath))
                {
                    return SetupType.InnoSetup;
                }
                else if (IsBurn(filePath))
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
    }
}
