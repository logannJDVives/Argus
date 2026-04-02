using Argus.Entities;
using Microsoft.EntityFrameworkCore;

namespace Argus.Data
{
    public class ArgusDbContext : DbContext
    {
        public ArgusDbContext(DbContextOptions<ArgusDbContext> options) 
            : base(options)
        {
        }

        // DbSets
        public DbSet<Project> Projects { get; set; }
        public DbSet<ScanRun> ScanRuns { get; set; }
        public DbSet<DetectedSecret> DetectedSecrets { get; set; }
        public DbSet<SoftwareComponent> SoftwareComponents { get; set; }
        public DbSet<Vulnerability> Vulnerabilities { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Dit vertelt EF Core expliciet waar de database staat tijdens migrations
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=ArgusDb;Trusted_Connection=True;MultipleActiveResultSets=true;");
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Project - ScanRun relatie (1:N)
            modelBuilder.Entity<ScanRun>()
                .HasOne(sr => sr.Project)
                .WithMany(p => p.ScanRuns)
                .HasForeignKey(sr => sr.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // ScanRun - DetectedSecret relatie (1:N)
            modelBuilder.Entity<DetectedSecret>()
                .HasOne(ds => ds.ScanRun)
                .WithMany(sr => sr.Secrets)
                .HasForeignKey(ds => ds.ScanRunId)
                .OnDelete(DeleteBehavior.Cascade);

            // ScanRun - SoftwareComponent relatie (1:N)
            modelBuilder.Entity<SoftwareComponent>()
                .HasOne(sc => sc.ScanRun)
                .WithMany(sr => sr.Components)
                .HasForeignKey(sc => sc.ScanRunId)
                .OnDelete(DeleteBehavior.Cascade);

            // SoftwareComponent - Self-referencing dependency tree (1:N)
            modelBuilder.Entity<SoftwareComponent>()
                .HasOne(sc => sc.ParentComponent)
                .WithMany(sc => sc.ChildComponents)
                .HasForeignKey(sc => sc.ParentComponentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // SoftwareComponent - Vulnerability relatie (1:N)
            modelBuilder.Entity<Vulnerability>()
                .HasOne(v => v.SoftwareComponent)
                .WithMany(sc => sc.Vulnerabilities)
                .HasForeignKey(v => v.SoftwareComponentId)
                .OnDelete(DeleteBehavior.Cascade);

            // ========== PROJECT INDEXES ==========
            modelBuilder.Entity<Project>()
                .HasIndex(p => p.CreatedAt)
                .IsDescending();

            // ========== SCANRUN INDEXES ==========
            modelBuilder.Entity<ScanRun>()
                .HasIndex(sr => sr.ProjectId);

            modelBuilder.Entity<ScanRun>()
                .HasIndex(sr => sr.Status);

            modelBuilder.Entity<ScanRun>()
                .HasIndex(sr => new { sr.ProjectId, sr.Status })
                .IsDescending(false, true);

            modelBuilder.Entity<ScanRun>()
                .HasIndex(sr => sr.CreatedAt)
                .IsDescending();

            // ========== DETECTED SECRET INDEXES ==========
            modelBuilder.Entity<DetectedSecret>()
                .HasIndex(ds => ds.ScanRunId);

            modelBuilder.Entity<DetectedSecret>()
                .HasIndex(ds => new { ds.FilePath, ds.LineNumber })
                .IsUnique();

            modelBuilder.Entity<DetectedSecret>()
                .HasIndex(ds => ds.Severity);

            modelBuilder.Entity<DetectedSecret>()
                .HasIndex(ds => ds.Confidence);

            modelBuilder.Entity<DetectedSecret>()
                .HasIndex(ds => ds.RuleId);

            modelBuilder.Entity<DetectedSecret>()
                .HasIndex(ds => new { ds.IsReviewed, ds.IsFalsePositive });

            modelBuilder.Entity<DetectedSecret>()
                .HasIndex(ds => ds.Type);

            modelBuilder.Entity<DetectedSecret>()
                .HasIndex(ds => ds.Hash)
                .IsUnique();

            modelBuilder.Entity<DetectedSecret>()
                .HasIndex(ds => new { ds.ScanRunId, ds.Severity })
                .IsDescending(false, true);

            modelBuilder.Entity<DetectedSecret>()
                .HasIndex(ds => new { ds.ScanRunId, ds.IsFalsePositive });

            // ========== SOFTWARE COMPONENT INDEXES ==========
            modelBuilder.Entity<SoftwareComponent>()
                .HasIndex(sc => sc.ScanRunId);

            modelBuilder.Entity<SoftwareComponent>()
                .HasIndex(sc => sc.PackageUrl)
                .IsUnique();

            modelBuilder.Entity<SoftwareComponent>()
                .HasIndex(sc => sc.HasKnownVulnerabilities);

            modelBuilder.Entity<SoftwareComponent>()
                .HasIndex(sc => sc.ParentComponentId);

            modelBuilder.Entity<SoftwareComponent>()
                .HasIndex(sc => new { sc.Name, sc.Version });

            modelBuilder.Entity<SoftwareComponent>()
                .HasIndex(sc => sc.Type);

            modelBuilder.Entity<SoftwareComponent>()
                .HasIndex(sc => sc.IsTransitive);

            modelBuilder.Entity<SoftwareComponent>()
                .HasIndex(sc => new { sc.ScanRunId, sc.HasKnownVulnerabilities });

            modelBuilder.Entity<SoftwareComponent>()
                .HasIndex(sc => new { sc.ScanRunId, sc.IsTransitive });

            // ========== VULNERABILITY INDEXES ==========
            modelBuilder.Entity<Vulnerability>()
                .HasIndex(v => v.SoftwareComponentId);

            modelBuilder.Entity<Vulnerability>()
                .HasIndex(v => v.CveId)
                .IsUnique();

            modelBuilder.Entity<Vulnerability>()
                .HasIndex(v => v.Severity);

            modelBuilder.Entity<Vulnerability>()
                .HasIndex(v => new { v.SoftwareComponentId, v.Severity });
        }
    }
}
