using System.Collections.Generic;

namespace InstallerStudio.Providers.InnoSetup.Models
{
    public class InnoType
    {
        public InnoType()
        {
            Flags = [];
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public List<string> Flags { get; private set; }
    }
}
