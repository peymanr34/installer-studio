using System.Collections.Generic;

namespace InstallerStudio.Providers.InnoSetup.Models
{
    public class InnoScript
    {
        public InnoScript()
        {
            Setup = [];
            Languages = [];
            Types = [];
            Components = [];
            Files = [];
            Runs = [];
        }

        public Dictionary<string, string> Setup { get; private set; }

        public List<InnoLanguage> Languages { get; private set; }

        public List<InnoType> Types { get; private set; }

        public List<InnoComponent> Components { get; private set; }

        public List<InnoFile> Files { get; private set; }

        public List<InnoRun> Runs { get; private set; }
    }
}
