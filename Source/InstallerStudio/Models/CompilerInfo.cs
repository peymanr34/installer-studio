using System;

namespace InstallerStudio.Models
{
    public class CompilerInfo
    {
        public string Name { get; set; }

        public Version SupportedVersion { get; set; }

        public CompilerType CompilerType { get; set; }

        public string ScriptFileExtension { get; set; }

        public string DownloadUrl { get; set; }
    }
}
