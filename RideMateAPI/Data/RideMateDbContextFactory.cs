using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using RideMateAPI.Infrastructure;

namespace RideMateAPI.Data
{
    // Design-time factory for EF tools when Program.cs can't be used to create the DbContext
    public class RideMateDbContextFactory : IDesignTimeDbContextFactory<RideMateDbContext>
    {
        public RideMateDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<RideMateDbContext>();

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var conn = PostgresConnectionString.FromConfiguration(config);

            if (string.IsNullOrWhiteSpace(conn))
                throw new InvalidOperationException("No connection string configured for RideMateDbContext. Set RIDE_MATE_CONNECTION, DATABASE_URL or add DefaultConnection to appsettings.json.");

            builder.UseNpgsql(conn);
            return new RideMateDbContext(builder.Options);
        }
    }
}
