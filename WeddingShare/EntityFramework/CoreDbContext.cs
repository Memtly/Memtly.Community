using Microsoft.EntityFrameworkCore;
using WeddingShare.EntityFramework.Models;
using WeddingShare.Enums;

namespace WeddingShare.EntityFramework
{
    public class CoreDbContext : DbContext
    {
        public CoreDbContext(DbContextOptions<CoreDbContext> options)
            : base(options) 
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Gallery> Galleries { get; set; }
        public DbSet<GalleryItem> GalleryItems { get; set; }
        public DbSet<GalleryLike> GalleryLikes { get; set; }
        public DbSet<GallerySetting> GallerySettings { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<CustomResource> CustomResources { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            mb.Entity<User>(e =>
            {
                e.HasIndex(x => x.Username).IsUnique();
                e.Property(x => x.Username).HasMaxLength(10);
                e.HasIndex(x => x.EmailAddress).IsUnique();
                e.Property(x => x.EmailAddress).HasMaxLength(200);
                e.Property(x => x.Firstname).HasMaxLength(50);
                e.Property(x => x.Lastname).HasMaxLength(50);
                e.Property(x => x.Password).HasMaxLength(500);
                e.Property(x => x.MultiFactorAuthToken).HasMaxLength(2000);
                e.Property(x => x.ActionAuthCode).HasMaxLength(2000);
                e.Property(x => x.Level).HasDefaultValue(UserLevel.Free);
                e.Property(x => x.State).HasDefaultValue(AccountState.PendingActivation);
                e.Property(x => x.FailedLoginCount).HasDefaultValue(0);
                e.Property(x => x.LockoutUntil).HasConversion(
                    v => v.HasValue ? v.Value.UtcTicks : (long?)null,
                    v => v.HasValue ? new DateTimeOffset(v.Value, TimeSpan.Zero) : (DateTimeOffset?)null
                );
                e.Property(x => x.CreatedAt).HasConversion(
                    v => v.UtcTicks,
                    v => new DateTimeOffset(v, TimeSpan.Zero)
                );
            });

            mb.Entity<Gallery>(e =>
            {
                e.HasIndex(x => x.Identifier).IsUnique();
                e.Property(x => x.Identifier).HasMaxLength(32);
                e.Property(x => x.Name).HasMaxLength(100);
                e.Property(x => x.SecretKey).HasMaxLength(500);
                e.Property(x => x.CreatedAt).HasConversion(
                    v => v.UtcTicks,
                    v => new DateTimeOffset(v, TimeSpan.Zero)
                );

                e.Ignore(x => x.IsSecure);

                e.HasOne(x => x.User)
                 .WithMany()
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            mb.Entity<GalleryItem>(e =>
            {
                e.Property(x => x.Title).HasMaxLength(100);
                e.Property(x => x.UploadedBy).HasMaxLength(100);
                e.Property(x => x.Checksum).HasMaxLength(1000);
                e.Property(x => x.FileSize).HasDefaultValue(0);
                e.Property(x => x.State).HasDefaultValue(GalleryItemState.Pending);
                e.Property(x => x.Type).HasDefaultValue(MediaType.Unknown);
                e.Property(x => x.Orientation).HasDefaultValue(ImageOrientation.Unknown);
                e.Property(x => x.CreatedAt).HasConversion(
                    v => v.UtcTicks,
                    v => new DateTimeOffset(v, TimeSpan.Zero)
                );

                e.HasOne(x => x.Gallery)
                 .WithMany(g => g.Items)
                 .HasForeignKey(x => x.GalleryId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            mb.Entity<GalleryLike>(e =>
            {
                e.Property(x => x.CreatedAt).HasConversion(
                    v => v.UtcTicks,
                    v => new DateTimeOffset(v, TimeSpan.Zero)
                );

                e.HasOne(x => x.GalleryItem)
                 .WithMany(gi => gi.Likes)
                 .HasForeignKey(x => x.GalleryItemId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.User)
                 .WithMany()
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            mb.Entity<GallerySetting>(e =>
            {
                e.Property(x => x.Value).HasMaxLength(1000);
                e.Property(x => x.CreatedAt).HasConversion(
                    v => v.UtcTicks,
                    v => new DateTimeOffset(v, TimeSpan.Zero)
                );

                e.HasOne(x => x.Gallery)
                 .WithMany(g => g.Settings)
                 .HasForeignKey(x => x.GalleryId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Setting)
                 .WithMany()
                 .HasForeignKey(x => x.SettingId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            mb.Entity<Setting>(e =>
            {
                e.HasIndex(x => x.Key).IsUnique();
                e.Property(x => x.Key).HasMaxLength(255);
                e.Property(x => x.Value).HasMaxLength(1000);
                e.Property(x => x.CreatedAt).HasConversion(
                    v => v.UtcTicks,
                    v => new DateTimeOffset(v, TimeSpan.Zero)
                );
            });

            mb.Entity<CustomResource>(e =>
            {
                e.Property(x => x.Filename).HasMaxLength(2000);
                e.Property(x => x.CreatedAt).HasConversion(
                    v => v.UtcTicks,
                    v => new DateTimeOffset(v, TimeSpan.Zero)
                );

                e.HasOne(x => x.User)
                 .WithMany()
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            mb.Entity<AuditLog>(e =>
            {
                e.Property(x => x.Message).HasMaxLength(2000);
                e.Property(x => x.Severity).HasDefaultValue(AuditSeverity.Information);
                e.Property(x => x.CreatedAt).HasConversion(
                    v => v.UtcTicks,
                    v => new DateTimeOffset(v, TimeSpan.Zero)
                );

                e.HasOne(x => x.User)
                 .WithMany()
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}