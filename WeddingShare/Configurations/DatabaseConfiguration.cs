using System.Collections;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using WeddingShare.Constants;
using WeddingShare.EntityFramework;
using WeddingShare.Enums;
using WeddingShare.Helpers;
using WeddingShare.Helpers.Database;
using WeddingShare.Models.Database;

namespace WeddingShare.Configurations
{
    internal static class DatabaseConfiguration
    {
        public static void AddDatabaseConfiguration(this IServiceCollection services)
        {
            var config = services.BuildServiceProvider().GetRequiredService<IConfigHelper>();
            
            var provider = config.GetOrDefault(Database.Type, "sqlite");
            var connString = config.GetOrDefault(Database.ConnectionString, "Data Source=./config/wedding-share.db");
            var assemblyName = typeof(CoreDbContext).Assembly.GetName().Name;

            services.AddDbContext<CoreDbContext>(options =>
            {
                switch (provider.ToLower())
                {
                    case "sqlite":
                        options.UseSqlite(connString, x =>
                        {
                            x.MigrationsAssembly(assemblyName);
                            x.MigrationsHistoryTable($"__EFMigrationsHistory_{provider}");
                        });
                        break;
                    case "mysql":
                    case "mariadb":
                        options.UseMySql(connString, ServerVersion.AutoDetect(connString), x =>
                        {
                            x.MigrationsAssembly(assemblyName);
                            x.MigrationsHistoryTable($"__EFMigrationsHistory_{provider}");
                        });
                        break;
                    case "mssql":
                        options.UseSqlServer(connString, x =>
                        {
                            x.MigrationsAssembly(assemblyName);
                            x.MigrationsHistoryTable($"__EFMigrationsHistory_{provider}");
                        });
                        break;
                    case "postgres":
                        options.UseNpgsql(connString, x =>
                        {
                            x.MigrationsAssembly(assemblyName);
                            x.MigrationsHistoryTable($"__EFMigrationsHistory_{provider}");
                        });
                        break;
                    default:
                        throw new InvalidOperationException($"Unsupported database provider: '{provider}'. Supported: sqlite, mysql, mariadb, mssql, postgres");
                }

                options.ReplaceService<IMigrationsAssembly, CoreDbContextMigrationFilter>();
            });

            services.AddScoped<IDatabaseHelper, EFDatabaseHelper>();

            var ctx = services.BuildServiceProvider().GetRequiredService<CoreDbContext>();

            var dbProvider = ctx.Database.ProviderName;

            ctx.Database.Migrate();

            var encryption = services.BuildServiceProvider().GetRequiredService<IEncryptionHelper>();
            var logger = services.BuildServiceProvider().GetRequiredService<ILogger<Startup>>();

            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IDatabaseHelper>();

                InitializeDatabase(config, db, encryption, logger);
            }
        }

        private static void InitializeDatabase(IConfigHelper config, IDatabaseHelper database, IEncryptionHelper encryption, ILogger logger)
        {
            var isDemoMode = config.GetOrDefault(Settings.IsDemoMode, false);
            var username = !isDemoMode ? config.GetOrDefault(Settings.Account.Admin.Username, config.GetOrDefault(Settings.Account.Owner.Username, "admin")).ToLower() : "demo";
            var password = encryption.Encrypt(!isDemoMode ? config.GetOrDefault(Settings.Account.Admin.Password, config.GetOrDefault(Settings.Account.Owner.Password, "admin")) : "demo", username);
            var allowInsecureGalleries = config.GetOrDefault(Settings.Basic.AllowInsecureGalleries, true);
            var defaultSecretKey = config.GetOrDefault(Settings.Basic.DefaultGallerySecretKey, string.Empty);

            Task.Run(async () =>
            {
                var adminAccount = await database.GetUser(1);
                if (adminAccount == null)
                {
                    await database.AddUser(new UserModel
                    {
                        Id = 1,
                        Username = username,
                        Email = $"{username}@example.com",
                        Firstname = "Admin",
                        Lastname = "User",
                        Password = password,
                        State = AccountState.Active,
                        Level = UserLevel.Owner
                    });
                }
                else
                {
                    adminAccount.Username = username;
                    adminAccount.Password = password;
                    adminAccount.State = AccountState.Active;
                    adminAccount.Level = UserLevel.Owner;

                    await database.EditUser(adminAccount);
                }

                adminAccount = await database.GetUser(1);
                if (adminAccount != null)
                {
                    var defaultGallery = await database.GetGallery(1);
                    if (defaultGallery == null)
                    {
                        var secretKey = !string.IsNullOrWhiteSpace(defaultSecretKey) || allowInsecureGalleries ? defaultSecretKey : PasswordHelper.GenerateGallerySecretKey();
                        await database.AddGallery(new GalleryModel
                        {
                            Id = 1,
                            Identifier = "Default",
                            Name = "Default",
                            SecretKey = secretKey,
                            Owner = adminAccount.Id
                        });
                    }
                }

                if (config.GetOrDefault(Constants.Database.SyncFromConfig, false))
                {
                    logger.LogWarning($"Sync_From_Config set to true, wiping settings database and re-pulling values from config");
                    await database.DeleteAllSettings();
                }

                await ImportSettings(config, database, logger);

                await database.SetSetting(new SettingModel()
                {
                    Id = Settings.IsDemoMode.ToUpper(),
                    Value = isDemoMode.ToString()
                });

                await database.SetSetting(new SettingModel()
                {
                    Id = Settings.Themes.Default.ToUpper(),
                    Value = config.GetOrDefault(Settings.Themes.Default, Themes.AutoDetect.ToString())
                });

                if (config.GetOrDefault(Security.MultiFactor.ResetToDefault, false))
                {
                    await database.ResetMultiFactorToDefault();
                }
            }).ConfigureAwait(false).GetAwaiter().GetResult();

            if (config.GetOrDefault(Settings.Account.Owner.LogPassword, false))
            {
                logger.LogInformation($"Password: {password}");
            }
        }

        private static async Task ImportSettings(IConfigHelper config, IDatabaseHelper database, ILogger logger)
        {
            try
            {
                var galleries = (await database.GetGalleries())?.Where(x => !x.Identifier.Equals("All", StringComparison.OrdinalIgnoreCase));

                var settings = await database.GetAllSettings();
                if (settings == null || !settings.Any(setting => setting.Id.StartsWith(Settings.Basic.BaseKey, StringComparison.OrdinalIgnoreCase)))
                {
                    var systemKeys = GetAllKeys();
                    foreach (var key in systemKeys)
                    {
                        try
                        {
                            if (settings == null || !settings.Any(setting => setting.Id.Equals(key, StringComparison.OrdinalIgnoreCase)))
                            {
                                var configVal = config.Get(key);
                                if (!string.IsNullOrWhiteSpace(configVal))
                                {
                                    await database.AddSetting(new SettingModel()
                                    {
                                        Id = key,
                                        Value = configVal
                                    });
                                }
                            }
                        }
                        catch { }
                    }

                    if (galleries != null && galleries.Any())
                    {
                        var galleryKeys = GetKeys<Settings.Gallery>();
                        foreach (var gallery in galleries)
                        {
                            if (!string.IsNullOrWhiteSpace(gallery?.Name))
                            {
                                foreach (var key in galleryKeys)
                                {
                                    try
                                    {
                                        var galleryOverride = config.GetEnvironmentVariable(key, gallery.Name);
                                        if (!string.IsNullOrWhiteSpace(galleryOverride))
                                        {
                                            await database.AddSetting(new SettingModel()
                                            {
                                                Id = key,
                                                Value = galleryOverride
                                            }, gallery.Id);
                                        }
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                }

                // Protect any galleries without a secret key by forcing a new one
                if (galleries != null && galleries.Any())
                {
                    var allowInsecureGalleries = config.GetOrDefault(Settings.Basic.AllowInsecureGalleries, true);
                    if (!allowInsecureGalleries)
                    {
                        foreach (var gallery in galleries.Where(gallery => string.IsNullOrWhiteSpace(gallery.SecretKey)))
                        {
                            try
                            {
                                gallery.SecretKey = config.GetOrDefault(Settings.Basic.DefaultGallerySecretKey, allowInsecureGalleries ? string.Empty : PasswordHelper.GenerateGallerySecretKey());

                                await database.EditGallery(gallery);
                            }
                            catch { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to import settings at startup - {ex?.Message}", ex);
            }
        }

        private static IEnumerable<string> GetAllKeys()
        {
            var keys = new List<string>();

            try
            {
                keys.AddRange(GetKeys<BackgroundServices>());
                keys.AddRange(GetKeys<Notifications>());
                keys.AddRange(GetKeys<Security>());
                keys.AddRange(GetKeys<Settings>());
            }
            catch { }

            return keys.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct();
        }

        private static IEnumerable<string> GetKeys<T>(bool includeNesteted = true)
        {
            var keys = new List<string>();

            try
            {
                var obj = Activator.CreateInstance<T>();
                foreach (var val in GetConstants(typeof(T), includeNesteted))
                {
                    keys.Add((string)(val.GetValue(obj) ?? string.Empty));
                }
            }
            catch { }

            return keys.Where(x => !string.IsNullOrWhiteSpace(x) && !x.EndsWith(':'));
        }

        private static FieldInfo[] GetConstants(Type type, bool includeNesteted)
        {
            var constants = new ArrayList();

            try
            {
                if (includeNesteted)
                {
                    var classInfos = type.GetNestedTypes(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                    foreach (var ci in classInfos)
                    {
                        var consts = GetConstants(ci, includeNesteted);
                        if (consts != null && consts.Length > 0)
                        {
                            constants.AddRange(consts);
                        }
                    }
                }

                var fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                foreach (var fi in fieldInfos)
                {
                    if (fi.IsLiteral && !fi.IsInitOnly)
                    {
                        constants.Add(fi);
                    }
                }
            }
            catch { }

            return (FieldInfo[])constants.ToArray(typeof(FieldInfo));
        }
    }
}