using System;
using System.Collections.Generic;

namespace InstallerStudio.Data.Models
{
    public class Project : BaseEntity
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Publisher { get; set; }

        public string Website { get; set; }

        public string Version { get; set; }

        public Guid UniqueId { get; set; }

        public SetupType SetupType { get; set; }

        public List<Setup> Setups { get; set; }
    }
}
