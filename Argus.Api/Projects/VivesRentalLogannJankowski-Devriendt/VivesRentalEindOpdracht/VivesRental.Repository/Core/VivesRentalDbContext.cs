using Microsoft.EntityFrameworkCore;
using VivesRental.Model;
using VivesRental.Repository.Extensions;

namespace VivesRental.Repository.Core;

public class VivesRentalDbContext: DbContext
{
    public VivesRentalDbContext(DbContextOptions options): base(options)
    {
            
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<ArticleReservation> ArticleReservations => Set<ArticleReservation>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.RemovePluralizingTableNameConvention();
        
        // Customer configuratie - email moet uniek zijn voor authenticatie
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasIndex(c => c.Email).IsUnique();
        });
        
        base.OnModelCreating(modelBuilder);
    }
}