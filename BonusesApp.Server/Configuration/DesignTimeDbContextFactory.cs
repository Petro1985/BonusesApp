using System.Reflection;
using BonusesApp.Core.Infrastructure;
using BonusesApp.Server.Infrastructure;
using BonusesApp.Server.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BonusesApp.Server.Configuration;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=Data/bonuses.db;Cache=Shared;Mode=ReadWriteCreate";
        connectionString = SqliteDatabasePath.Prepare(connectionString, Directory.GetCurrentDirectory());

        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
        var migrationsAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;

        builder.UseSqlite(connectionString, b => b.MigrationsAssembly(migrationsAssembly));
        builder.AddInterceptors(new SqlitePragmaInterceptor());
        builder.UseOpenIddict();

        return new ApplicationDbContext(builder.Options, SystemUserIdAccessor.GetNewAccessor());
    }
}
