using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WeddingShare.EntityFramework
{
    public class CoreDbContextFactory : IDesignTimeDbContextFactory<CoreDbContext>
    {
        public CoreDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var provider = config[Constants.Database.Type]!;
            var assemblyName = typeof(CoreDbContext).Assembly.GetName().Name;

            var options = new DbContextOptionsBuilder<CoreDbContext>();
            switch (provider.ToLower())
            {
                case "sqlite":
                    var sqliteFile = $"_design_temp_{provider.ToLower()}.db";
                    if (File.Exists(sqliteFile))
                    {
                        File.Delete(sqliteFile);
                    }

                    options.UseSqlite($"Data Source={sqliteFile}", x =>
                    {
                        x.MigrationsAssembly(assemblyName);
                        x.MigrationsHistoryTable($"__EFMigrationsHistory_{provider}");
                    });
                    break;
                case "mysql":
                case "mariadb":
                    options.UseMySql($"Server=localhost;Database=_design_temp_{provider.ToLower()};", MySqlServerVersion.LatestSupportedServerVersion, x =>
                    {
                        x.MigrationsAssembly(assemblyName);
                        x.MigrationsHistoryTable($"__EFMigrationsHistory_{provider}");
                    });
                    break;
                case "mssql":
                    options.UseSqlServer($"Server=(localdb)\\mssqllocaldb;Database=_design_temp_{provider.ToLower()};", x =>
                        {
                            x.MigrationsAssembly(assemblyName);
                            x.MigrationsHistoryTable($"__EFMigrationsHistory_{provider}");
                        });
                    break;
                case "postgres":
                    options.UseNpgsql($"Host=localhost;Database=_design_temp_{provider.ToLower()};", x =>
                        {
                            x.MigrationsAssembly(assemblyName);
                            x.MigrationsHistoryTable($"__EFMigrationsHistory_{provider}");
                        });
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported database provider: '{provider}'. Supported: sqlite, mysql, mariadb, mssql, postgres");
            }

            return new CoreDbContext(options.Options);
        }
    }
}