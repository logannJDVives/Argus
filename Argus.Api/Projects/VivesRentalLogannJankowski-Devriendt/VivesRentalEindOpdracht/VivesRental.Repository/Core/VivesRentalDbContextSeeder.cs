using System.Security.Cryptography;
using System.Text;
using VivesRental.Enums;
using VivesRental.Model;

namespace VivesRental.Repository.Core;

public static class VivesRentalDbContextSeeder
{
    public static void Seed(VivesRentalDbContext context)
    {
        // Altijd controleren of admin bestaat
        EnsureAdminExists(context);

        if (!context.Customers.Any(c => c.Email != "admin"))
        {
            SeedCustomers(context);
        }

        if (!context.Products.Any())
        {
            SeedProducts(context);
        }
    }

    private static void EnsureAdminExists(VivesRentalDbContext context)
    {
        // Controleer of admin al bestaat
        var existingAdmin = context.Customers.FirstOrDefault(c => c.Email == "admin");
        
        if (existingAdmin == null)
        {
            // Admin account aanmaken: admin / admin
            var admin = new Customer
            {
                Id = Guid.NewGuid(),
                FirstName = "Admin",
                LastName = "User",
                Email = "admin",
                PhoneNumber = "",
                PasswordHash = HashPassword("admin"),
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            };

            context.Customers.Add(admin);
            context.SaveChanges();
        }
        else if (existingAdmin.PasswordHash == null)
        {
            // Admin bestaat maar heeft geen wachtwoord (oude data)
            existingAdmin.PasswordHash = HashPassword("admin");
            existingAdmin.Role = "Admin";
            existingAdmin.CreatedAt = DateTime.UtcNow;
            context.SaveChanges();
        }
    }

    private static void SeedCustomers(VivesRentalDbContext context)
    {
        var customer1 = new Customer
        {
            FirstName = "Jan",
            LastName = "Janssens",
            Email = "jan.janssens@example.com",
            PhoneNumber = "+32 471 12 34 56",
            Role = "User"
        };

        var customer2 = new Customer
        {
            FirstName = "Marie",
            LastName = "Peeters",
            Email = "marie.peeters@example.com",
            PhoneNumber = "+32 471 98 76 54",
            Role = "User"
        };

        var customer3 = new Customer
        {
            FirstName = "Tom",
            LastName = "De Vries",
            Email = "tom.devries@example.com",
            PhoneNumber = "+32 471 11 22 33",
            Role = "User"
        };

        context.Customers.AddRange(customer1, customer2, customer3);
        context.SaveChanges();
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private static void SeedProducts(VivesRentalDbContext context)
    {
        var laptop = new Product
        {
            Name = "Laptop",
            Description = "Dell XPS 15 - High Performance Laptop",
            Manufacturer = "Dell",
            Publisher = "Dell Inc.",
            RentalExpiresAfterDays = 30
        };

        var beamer = new Product
        {
            Name = "Beamer",
            Description = "HD Projector - 1080p",
            Manufacturer = "Epson",
            Publisher = "Epson Europe",
            RentalExpiresAfterDays = 14
        };

        var camera = new Product
        {
            Name = "Camera",
            Description = "Canon EOS R5 - Professional Camera",
            Manufacturer = "Canon",
            Publisher = "Canon Europe",
            RentalExpiresAfterDays = 7
        };

        context.Products.AddRange(laptop, beamer, camera);
        context.SaveChanges();

        var laptopArticle1 = new Article
        {
            ProductId = laptop.Id,
            Status = ArticleStatus.Normal
        };

        var laptopArticle2 = new Article
        {
            ProductId = laptop.Id,
            Status = ArticleStatus.Normal
        };

        var beamerArticle = new Article
        {
            ProductId = beamer.Id,
            Status = ArticleStatus.Normal
        };

        var cameraArticle = new Article
        {
            ProductId = camera.Id,
            Status = ArticleStatus.InRepair
        };

        context.Articles.AddRange(laptopArticle1, laptopArticle2, beamerArticle, cameraArticle);
        context.SaveChanges();
    }
}
