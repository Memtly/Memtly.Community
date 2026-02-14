using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;

namespace WeddingShare.EntityFramework
{
    public class CoreDbContextMigrationFilter : MigrationsAssembly
    {
        private readonly DbContext _context;

        public CoreDbContextMigrationFilter(ICurrentDbContext currentContext, IDbContextOptions options, IMigrationsIdGenerator idGenerator, IDiagnosticsLogger<DbLoggerCategory.Migrations> logger)
            : base(currentContext, options, idGenerator, logger)
        {
            _context = currentContext.Context;
        }

        public override IReadOnlyDictionary<string, TypeInfo> Migrations
        {
            get
            {
                var provider = _context.Database.ProviderName;

                var targetNamespace = provider?.ToLower() switch
                {
                    var p when p.Contains("sqlite") => "WeddingShare.Migrations.Sqlite",
                    var p when p.Contains("mysql") => "WeddingShare.Migrations.MySql",
                    var p when p.Contains("sqlserver") => "WeddingShare.Migrations.SqlServer",
                    var p when p.Contains("npgsql") || p.Contains("postgres") => "WeddingShare.Migrations.Postgres",
                    _ => throw new InvalidOperationException($"Unknown provider: {provider}")
                };

                return base.Migrations
                    .Where(m => m.Value.Namespace == targetNamespace)
                    .ToDictionary(m => m.Key, m => m.Value);
            }
        }
    }
}