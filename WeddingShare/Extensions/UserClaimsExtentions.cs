using System.Security.Claims;
using System.Security.Principal;
using WeddingShare.Enums;
using WeddingShare.Models;

namespace WeddingShare.Extensions
{
    public static class UserClaimsExtentions
    {
        public static int GetUserId(this IIdentity identity)
        {
            try
            {
                return int.Parse(((ClaimsIdentity)identity).Claims.FirstOrDefault(x => string.Equals(ClaimTypes.Sid, x.Type, StringComparison.OrdinalIgnoreCase))?.Value ?? "-1");
            }
            catch { }

            return -1;
        }

        public static UserLevel GetUserLevel(this IIdentity identity)
        {
            try
            {
                var level = ((ClaimsIdentity)identity).Claims.FirstOrDefault(x => string.Equals(ClaimTypes.Role, x.Type, StringComparison.OrdinalIgnoreCase))?.Value;
                foreach (UserLevel l in Enum.GetValues(typeof(UserLevel)))
                {
                    if (l.ToString().Equals(level, StringComparison.OrdinalIgnoreCase))
                    {
                        return l;
                    }
                }
            }
            catch { }

            return UserLevel.Free;
        }

        public static bool IsPrivilegedUser(this IIdentity identity)
        {
            try
            {
                var userLevel = identity?.GetUserLevel() ?? UserLevel.Free;
                
                return userLevel == UserLevel.Reviewer
                    || userLevel == UserLevel.Moderator
                    || userLevel == UserLevel.Admin;
            }
            catch { }

            return false;
        }

        public static bool IsBasicUser(this IIdentity identity)
        {
            return identity == null || identity.IsPrivilegedUser() == false;
        }

        public static Permissions GetUserPermissions(this IIdentity identity)
        {
            try
            {
                var level = identity.GetUserLevel();
                switch (level)
                {
                    case UserLevel.Free:
                        return new FreeUserPermissions();
                    case UserLevel.Paid:
                        return new PaidUserPermissions();
                    case UserLevel.Reviewer:
                        return new ReviewerPermissions();
                    case UserLevel.Moderator:
                        return new ModeratorPermissions();
                    case UserLevel.Admin:
                        return new AdminPermissions();
                    default: 
                        return new Permissions();
                }
            }
            catch { }

            return new Permissions();
        }

        public static int GetGalleryLimit(this IIdentity identity)
        {
            try
            {
                switch (identity.GetUserLevel())
                {
                    case UserLevel.Free:
                    case UserLevel.Reviewer:
                        return 0;
                    case UserLevel.Paid:
                        return 3;
                    case UserLevel.Moderator:
                    case UserLevel.Admin:
                        return int.MaxValue;
                }
            }
            catch { }

            return 0;
        }

        public static AccountTabs GetDefaultTab(this IIdentity identity)
        {
            var userPermissions = identity?.GetUserPermissions() ?? new Permissions();
            if (userPermissions.Review.HasFlag(ReviewPermissions.View))
            {
                return AccountTabs.Reviews;
            }
            else if (userPermissions.Gallery.HasFlag(GalleryPermissions.View))
            {
                return AccountTabs.Galleries;
            }
            else if (userPermissions.Users.HasFlag(UserPermissions.View))
            {
                return AccountTabs.Users;
            }
            else if (userPermissions.CustomResources.HasFlag(CustomResourcePermissions.View))
            {
                return AccountTabs.Resources;
            }
            else if (userPermissions.Settings.HasFlag(SettingsPermissions.View))
            {
                return AccountTabs.Settings;
            }
            else if (userPermissions.Audit.HasFlag(AuditPermissions.View))
            {
                return AccountTabs.Audit;
            }

            return AccountTabs.Reviews;
        }

        public static bool CanEdit(this IIdentity identity, Enum type, int? ownerId)
        {
            try
            {
                if (identity != null)
                {
                    var level = identity.GetUserLevel();
                    var permissions = identity.GetUserPermissions();

                    if (identity.IsPrivilegedUser())
                    {
                        return true;
                    }
                    else
                    {
                        var hasPermissions = false;
                        if (type.GetType() == typeof(ReviewPermissions))
                        {
                            hasPermissions = permissions.Review.HasFlag(type);
                        }
                        else if (type.GetType() == typeof(GalleryPermissions))
                        {
                            hasPermissions = permissions.Gallery.HasFlag(type);
                        }
                        else if (type.GetType() == typeof(UserPermissions))
                        {
                            hasPermissions = permissions.Users.HasFlag(type);
                        }
                        else if (type.GetType() == typeof(CustomResourcePermissions))
                        {
                            hasPermissions = permissions.CustomResources.HasFlag(type);
                        }
                        else if (type.GetType() == typeof(SettingsPermissions))
                        {
                            hasPermissions = permissions.Settings.HasFlag(type);
                        }
                        else if (type.GetType() == typeof(AuditPermissions))
                        {
                            hasPermissions = permissions.Audit.HasFlag(type);
                        }
                        else if (type.GetType() == typeof(DataPermissions))
                        {
                            hasPermissions = permissions.Data.HasFlag(type);
                        }

                        return hasPermissions && (ownerId != null && identity.GetUserId() == ownerId);
                    }
                }
            }
            catch { }

            return false;
        }
    }
}