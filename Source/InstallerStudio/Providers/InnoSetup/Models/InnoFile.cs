using System.Collections.Generic;

namespace InstallerStudio.Providers.InnoSetup.Models
{
    public class InnoFile
    {
        public InnoFile()
        {
            Flags = [];
            Checks = [];
            Components = [];
        }

        public string Source { get; set; }

        public string DestDir { get; set; }

        public List<string> Components { get; private set; }

        public List<string> Flags { get; private set; }

        public List<string> Checks { get; private set; }
    }
}
