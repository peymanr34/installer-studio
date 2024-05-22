using InstallerStudio.Data.Models;

namespace InstallerStudio
{
    public static class ModelExtensions
    {
        public static string GetIdentifier(this Setup item)
        {
            return $"Setup{item.Id}";
        }
    }
}