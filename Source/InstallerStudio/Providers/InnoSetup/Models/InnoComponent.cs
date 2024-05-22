using System.Collections.Generic;

namespace InstallerStudio.Providers.InnoSetup.Models
{
    public class InnoComponent
    {
        public InnoComponent()
        {
            Types = [];
            Checks = [];
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public List<string> Types { get; private set; }

        public List<string> Checks { get; private set; }
    }
}
