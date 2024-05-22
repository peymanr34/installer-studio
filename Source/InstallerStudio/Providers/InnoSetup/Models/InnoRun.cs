using System.Collections.Generic;

namespace InstallerStudio.Providers.InnoSetup.Models
{
    public class InnoRun
    {
        public InnoRun()
        {
            Flags = [];
            Checks = [];
            Components = [];
        }

        public string FileName { get; set; }

        public string Parameters { get; set; }

        public string StatusMsg { get; set; }

        public List<string> Flags { get; private set; }

        public List<string> Checks { get; private set; }

        public List<string> Components { get; private set; }
    }
}
