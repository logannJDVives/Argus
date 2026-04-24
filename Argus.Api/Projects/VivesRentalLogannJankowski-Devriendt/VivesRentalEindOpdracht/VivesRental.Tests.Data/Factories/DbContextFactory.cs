using Microsoft.EntityFrameworkCore;
using VivesRental.Repository.Core;

namespace VivesRental.Tests.Data.Factories;

public static class DbContextFactory
{
    public static VivesRentalDbContext CreateInstance(string databaseName)
    {
        var options = new DbContextOptionsBuilder<VivesRentalDbContext>()
            .UseSqlServer($"Server=(localdb)\\mssqllocaldb;Database=VivesRentalTest_{databaseName};Trusted_Connection=True;MultipleActiveResultSets=true")
            .Options;

        var context = new VivesRentalDbContext(options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        
        return context;
    }
}