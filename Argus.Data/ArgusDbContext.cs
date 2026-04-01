using Argus.Entities;
using Microsoft.EntityFrameworkCore;
namespace Argus.Data
{
    public class ArgusDbContext : DbContext
    {
        public ArgusDbContext(DbContextOptions<ArgusDbContext> options) : base(options)
        {
        }


        // DbSets
        public DbSet<Project> Projects { get; set; }
        public DbSet<DetectedSecret> DetectedSecrets { get; set; }
        public DbSet<SoftwareComponent> SoftwareComponents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Project - DetectedSecret relatie (1:N)
            modelBuilder.Entity<DetectedSecret>()
                .HasOne(ds => ds.Project)
                .WithMany(p => p.Secrets)
                .HasForeignKey(ds => ds.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Project - SoftwareComponent relatie (1:N)
            modelBuilder.Entity<SoftwareComponent>()
                .HasOne(sc => sc.Project)
                .WithMany(p => p.Components)
                .HasForeignKey(sc => sc.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexen voor betere query performance
            modelBuilder.Entity<DetectedSecret>()
                .HasIndex(ds => ds.ProjectId);

            modelBuilder.Entity<DetectedSecret>()
                .HasIndex(ds => new { ds.FilePath, ds.LineNumber });

            modelBuilder.Entity<SoftwareComponent>()
                .HasIndex(sc => sc.ProjectId);

            modelBuilder.Entity<SoftwareComponent>()
                .HasIndex(sc => sc.PackageUrl);
        }
    }
}
