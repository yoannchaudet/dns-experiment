using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SharedModel;

public class DatabaseContext(string connectionString, ILoggerFactory loggerFactoryFactory) : DbContext
{
    public DbSet<JournalOperation> JournalOperations { get; set; }

    private string ConnectionString { get; } = connectionString;

    private ILoggerFactory LoggerFactory { get; } = loggerFactoryFactory;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(ConnectionString);
        // Too verbose.
        // .UseLoggerFactory(LoggerFactory);
    }
}