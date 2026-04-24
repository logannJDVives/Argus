using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VivesRental.Repository.Core;

public class VivesRentalDbContextFactory : IDesignTimeDbContextFactory<VivesRentalDbContext>
{
    public VivesRentalDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<VivesRentalDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=VivesRentalDb;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new VivesRentalDbContext(optionsBuilder.Options);
    }
}
