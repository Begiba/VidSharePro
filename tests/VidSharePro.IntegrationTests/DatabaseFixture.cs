using Microsoft.EntityFrameworkCore;
using VidSharePro.Infrastructure.Persistence;

namespace VidSharePro.IntegrationTests;

public class DatabaseFixture : IDisposable
{
    public AppDbContext Context { get; private set; }

    public DatabaseFixture()
    {
        // Option A: Use a real SQL Server test database
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=VidSharePro_Test;Trusted_Connection=True;")
            .Options;

        Context = new AppDbContext(options);

        // Ensure the database is clean and has the latest schema
        Context.Database.EnsureDeleted();
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
}