using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Persistence;

/// <summary>
/// Fábrica de diseño para migraciones con CLI (dotnet ef).
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=PulseraPTR;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");
        return new AppDbContext(optionsBuilder.Options);
    }
}
