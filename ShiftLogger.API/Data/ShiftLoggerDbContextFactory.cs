using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ShiftLogger.API.Data;

public class ShiftLoggerDbContextFactory : IDesignTimeDbContextFactory<ShiftLoggerDbContext>
{
    public ShiftLoggerDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ShiftLoggerDbContext>();
        var connectionString = config.GetConnectionString("DatabaseConnection");
        optionsBuilder.UseSqlServer(connectionString);

        return new ShiftLoggerDbContext(optionsBuilder.Options);
    }
}
