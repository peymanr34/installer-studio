using InstallerStudio.Data.Interceptors;
using InstallerStudio.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace InstallerStudio.Data
{
    public class DatabaseContext(DbContextOptions<DatabaseContext> options)
        : DbContext(options)
    {
        public DbSet<Project> Projects { get; set; }

        public DbSet<Setup> Setups { get; set; }

        public DbSet<SetupAdditional> Additionals { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder?.AddInterceptors(new DateTimeInterceptor());
        }
    }
}
