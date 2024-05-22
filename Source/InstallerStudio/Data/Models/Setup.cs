using System.Collections.Generic;

namespace InstallerStudio.Data.Models
{
    public class Setup : BaseEntity
    {
        public string Name { get; set; }

        public string Version { get; set; }

        public string Description { get; set; }

        public string FilePath { get; set; }

        public string Arguments { get; set; }

        public bool IsX86 { get; set; }

        public bool IsX64 { get; set; }

        public bool IsArm64 { get; set; }

        public int ProjectId { get; set; }

        public Project Project { get; set; }

        public List<SetupAdditional> Additionals { get; set; }
    }
}
