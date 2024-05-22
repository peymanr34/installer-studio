namespace InstallerStudio.Data.Models
{
    public class SetupAdditional : BaseEntity
    {
        public string Path { get; set; }

        public bool IsDirectory { get; set; }

        public int SetupId { get; set; }

        public Setup Setup { get; set; }
    }
}
