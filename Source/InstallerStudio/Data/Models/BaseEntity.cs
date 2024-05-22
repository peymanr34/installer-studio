using System;
using System.ComponentModel.DataAnnotations;

namespace InstallerStudio.Data.Models
{
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreatedDateUtc { get; set; }

        public DateTime ModifiedDateUtc { get; set; }
    }
}
