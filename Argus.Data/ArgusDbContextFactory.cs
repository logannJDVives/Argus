using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace Argus.Data
{
    public class ArgusDbContextFactory : IDesignTimeDbContextFactory<ArgusDbContext>
    {
        public ArgusDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ArgusDbContext>();

            optionsBuilder.UseSqlServer(
                "Server=(localdb)\\MSSQLLocalDB;Database=ArgusDb;Trusted_Connection=True;"
            );

            return new ArgusDbContext(optionsBuilder.Options);
        }

    }
}
