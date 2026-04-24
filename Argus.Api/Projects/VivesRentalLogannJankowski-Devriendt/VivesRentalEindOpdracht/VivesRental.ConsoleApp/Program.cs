using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VivesRental.Repository.Core;

namespace VivesRental.ConsoleApp;

class Program
{
    static void Main(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Setup dependency injection
        var services = new ServiceCollection();

        // Register DbContext
        services.AddDbContext<VivesRentalDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("VivesRental")));

        var serviceProvider = services.BuildServiceProvider();

        // Get DbContext and apply migrations
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<VivesRentalDbContext>();

        Console.WriteLine("Applying migrations...");
        context.Database.Migrate();
        Console.WriteLine("Migrations applied successfully.");

        Console.WriteLine("Seeding database...");
        VivesRentalDbContextSeeder.Seed(context);
        Console.WriteLine("Database seeded successfully.");

        Console.WriteLine("\nDatabase is ready!");
        Console.WriteLine("Connection string: " + configuration.GetConnectionString("VivesRental"));
        Console.WriteLine("\nYou can now view the database in SQL Server Management Studio.");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
