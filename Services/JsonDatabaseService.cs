using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using ShoesStore.Models;

namespace ShoesStore.Services
{
    /// <summary>
    /// Thread-safe JSON-backed store for products, users, roles, and user-role links.
    /// Every read/write goes through <see cref="Read{T}"/> or <see cref="Mutate"/> so
    /// callers don't have to manage locking or remember to persist. Find* methods
    /// return cloned snapshots; mutations must go through <see cref="Mutate"/>.
    /// </summary>
    public class JsonDatabaseService
    {
        public const string AdminRole = "Admin";
        public const string DefaultUserRole = "User";
        private const char UserRoleSeparator = '|';

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        private readonly string _dataFilePath;
        private readonly object _lock = new();
        private ApplicationDbData _data = new();

        public JsonDatabaseService(string dataFilePath)
        {
            _dataFilePath = dataFilePath;
            LoadData();
        }

        // ---------- Persistence ----------

        public void Save()
        {
            lock (_lock)
            {
                SaveInternal();
            }
        }

        private void LoadData()
        {
            lock (_lock)
            {
                if (!File.Exists(_dataFilePath))
                {
                    _data = new ApplicationDbData();
                    SaveInternal();
                    return;
                }

                var json = File.ReadAllText(_dataFilePath);
                try
                {
                    _data = JsonSerializer.Deserialize<ApplicationDbData>(json, JsonOptions)
                            ?? new ApplicationDbData();
                }
                catch (JsonException)
                {
                    // Corrupt file — back it up and start fresh so the app can boot.
                    var backup = _dataFilePath + ".corrupt-" +
                                 DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                    try { File.Copy(_dataFilePath, backup, overwrite: true); } catch { /* best effort */ }
                    _data = new ApplicationDbData();
                    SaveInternal();
                    return;
                }

                MigrateUserRoleLinks(_data);
                EnsureNextProductId(_data);
            }
        }

        private void SaveInternal()
        {
            var json = JsonSerializer.Serialize(_data, JsonOptions);
            // Write to a temp file first, then rename — protects against
            // partial writes on crash / power loss.
            var tempPath = _dataFilePath + ".tmp";
            File.WriteAllText(tempPath, json);
            File.Move(tempPath, _dataFilePath, overwrite: true);
        }

        // Old format: "userName|RoleName". New format: "userId|roleId".
        // Migration is best-effort: any link we can't resolve is dropped.
        private static void MigrateUserRoleLinks(ApplicationDbData data)
        {
            if (data.UserRoles.Count == 0) return;

            var migrated = new List<string>(data.UserRoles.Count);
            foreach (var entry in data.UserRoles)
            {
                var parts = entry.Split(UserRoleSeparator, 2);
                if (parts.Length != 2) continue;

                var left = parts[0];
                var right = parts[1];

                // Already in id|id form?
                var userById = data.Users.FirstOrDefault(u => u.Id == left);
                var roleById = data.Roles.FirstOrDefault(r => r.Id == right);
                if (userById != null && roleById != null)
                {
                    migrated.Add(entry);
                    continue;
                }

                // Convert "userName|RoleName" → "userId|roleId".
                var user = data.Users.FirstOrDefault(u => u.UserName == left);
                var role = data.Roles.FirstOrDefault(r =>
                    string.Equals(r.Name, right, StringComparison.OrdinalIgnoreCase));
                if (user != null && role != null)
                {
                    migrated.Add(user.Id + UserRoleSeparator + role.Id);
                }
            }
            data.UserRoles = migrated.Distinct().ToList();
        }

        private static void EnsureNextProductId(ApplicationDbData data)
        {
            var maxId = data.Products.Count == 0 ? 0 : data.Products.Max(p => p.Id);
            if (data.NextProductId <= maxId)
            {
                data.NextProductId = maxId + 1;
            }
        }

        // ---------- Locking helpers ----------

        private T Read<T>(Func<ApplicationDbData, T> read)
        {
            lock (_lock)
            {
                return read(_data);
            }
        }

        private void Mutate(Action<ApplicationDbData> mutate)
        {
            lock (_lock)
            {
                mutate(_data);
                SaveInternal();
            }
        }

        private bool MutateIf(Func<ApplicationDbData, bool> mutate)
        {
            lock (_lock)
            {
                if (!mutate(_data))
                {
                    return false;
                }
                SaveInternal();
                return true;
            }
        }

        // ---------- Cloning ----------
        // Find* methods return clones so callers can never mutate internal state
        // without going through Mutate(). Updates must call SaveProduct/SaveUser/SaveRole.

        private static Product Clone(Product src) => new()
        {
            Id = src.Id,
            Name = src.Name,
            Description = src.Description,
            Price = src.Price,
            OldPrice = src.OldPrice,
            Emoji = src.Emoji,
            Category = src.Category,
            Material = src.Material,
            Color = src.Color,
            CreatedAt = src.CreatedAt,
            UpdatedAt = src.UpdatedAt,
            Sizes = src.Sizes.Select(s => new ProductSize { Size = s.Size, InStock = s.InStock }).ToList()
        };

        private static ApplicationUser Clone(ApplicationUser src) => new()
        {
            Id = src.Id,
            UserName = src.UserName,
            NormalizedUserName = src.NormalizedUserName,
            Email = src.Email,
            NormalizedEmail = src.NormalizedEmail,
            EmailConfirmed = src.EmailConfirmed,
            PasswordHash = src.PasswordHash,
            SecurityStamp = src.SecurityStamp,
            ConcurrencyStamp = src.ConcurrencyStamp,
            PhoneNumber = src.PhoneNumber,
            PhoneNumberConfirmed = src.PhoneNumberConfirmed,
            TwoFactorEnabled = src.TwoFactorEnabled,
            LockoutEnd = src.LockoutEnd,
            LockoutEnabled = src.LockoutEnabled,
            AccessFailedCount = src.AccessFailedCount
        };

        private static IdentityRole Clone(IdentityRole src) => new()
        {
            Id = src.Id,
            Name = src.Name,
            NormalizedName = src.NormalizedName,
            ConcurrencyStamp = src.ConcurrencyStamp
        };

        // ---------- Collections (snapshots) ----------

        public IReadOnlyList<Product> Products =>
            Read(d => d.Products.Select(Clone).ToList());

        // ---------- Products ----------

        public Product? FindProductById(int id) =>
            Read(d =>
            {
                var p = d.Products.FirstOrDefault(x => x.Id == id);
                return p == null ? null : Clone(p);
            });

        public bool ProductNameExists(string name, int? excludeId = null) =>
            Read(d => d.Products.Any(p =>
                string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase) &&
                (!excludeId.HasValue || p.Id != excludeId.Value)));

        public void AddProduct(Product product) =>
            Mutate(d =>
            {
                if (product.Id == 0)
                {
                    EnsureNextProductId(d);
                    product.Id = d.NextProductId++;
                }
                d.Products.Add(Clone(product));
            });

        public bool RemoveProduct(int id) =>
            MutateIf(d => d.Products.RemoveAll(p => p.Id == id) > 0);

        public bool SaveProduct(Product product) =>
            MutateIf(d =>
            {
                var index = d.Products.FindIndex(p => p.Id == product.Id);
                if (index < 0) return false;
                d.Products[index] = Clone(product);
                return true;
            });

        // ---------- Users ----------

        public void AddUser(ApplicationUser user) => Mutate(d => d.Users.Add(Clone(user)));

        public bool RemoveUserById(string id) =>
            MutateIf(d =>
            {
                if (d.Users.RemoveAll(u => u.Id == id) == 0) return false;
                // Cascade-delete role links to keep the data consistent.
                var prefix = id + UserRoleSeparator;
                d.UserRoles.RemoveAll(r => r.StartsWith(prefix, StringComparison.Ordinal));
                return true;
            });

        public bool SaveUser(ApplicationUser user) =>
            MutateIf(d =>
            {
                var index = d.Users.FindIndex(u => u.Id == user.Id);
                if (index < 0) return false;
                d.Users[index] = Clone(user);
                return true;
            });

        public ApplicationUser? FindUserById(string id) =>
            Read(d =>
            {
                var u = d.Users.FirstOrDefault(x => x.Id == id);
                return u == null ? null : Clone(u);
            });

        public ApplicationUser? FindUserByNormalizedName(string normalizedUserName) =>
            Read(d =>
            {
                var u = d.Users.FirstOrDefault(x => x.NormalizedUserName == normalizedUserName);
                return u == null ? null : Clone(u);
            });

        public ApplicationUser? FindUserByNormalizedEmail(string normalizedEmail) =>
            Read(d =>
            {
                var u = d.Users.FirstOrDefault(x => x.NormalizedEmail == normalizedEmail);
                return u == null ? null : Clone(u);
            });

        // ---------- Roles ----------

        public void AddRole(IdentityRole role) => Mutate(d => d.Roles.Add(Clone(role)));

        public bool RemoveRoleById(string id) =>
            MutateIf(d =>
            {
                if (d.Roles.RemoveAll(r => r.Id == id) == 0) return false;
                var suffix = UserRoleSeparator + id;
                d.UserRoles.RemoveAll(r => r.EndsWith(suffix, StringComparison.Ordinal));
                return true;
            });

        public bool SaveRole(IdentityRole role) =>
            MutateIf(d =>
            {
                var index = d.Roles.FindIndex(r => r.Id == role.Id);
                if (index < 0) return false;
                d.Roles[index] = Clone(role);
                return true;
            });

        public IdentityRole? FindRoleById(string id) =>
            Read(d =>
            {
                var r = d.Roles.FirstOrDefault(x => x.Id == id);
                return r == null ? null : Clone(r);
            });

        public IdentityRole? FindRoleByNormalizedName(string normalizedRoleName) =>
            Read(d =>
            {
                var r = d.Roles.FirstOrDefault(x => x.NormalizedName == normalizedRoleName);
                return r == null ? null : Clone(r);
            });

        // ---------- User-role links ----------

        public bool AddUserRoleLink(string userId, string roleId) =>
            MutateIf(d =>
            {
                var key = userId + UserRoleSeparator + roleId;
                if (d.UserRoles.Contains(key)) return false;
                d.UserRoles.Add(key);
                return true;
            });

        public bool RemoveUserRoleLink(string userId, string roleId) =>
            MutateIf(d => d.UserRoles.Remove(userId + UserRoleSeparator + roleId));

        public bool ContainsUserRoleLink(string userId, string roleId) =>
            Read(d => d.UserRoles.Contains(userId + UserRoleSeparator + roleId));

        public List<string> GetRoleNamesForUser(string userId) =>
            Read(d =>
            {
                var prefix = userId + UserRoleSeparator;
                var roleIds = d.UserRoles
                    .Where(r => r.StartsWith(prefix, StringComparison.Ordinal))
                    .Select(r => r.Substring(prefix.Length))
                    .ToHashSet();
                return d.Roles
                    .Where(r => roleIds.Contains(r.Id) && r.Name != null)
                    .Select(r => r.Name!)
                    .ToList();
            });

        public List<ApplicationUser> GetUsersInRole(string normalizedRoleName) =>
            Read(d =>
            {
                var role = d.Roles.FirstOrDefault(r => r.NormalizedName == normalizedRoleName);
                if (role == null) return new List<ApplicationUser>();

                var suffix = UserRoleSeparator + role.Id;
                var userIds = d.UserRoles
                    .Where(r => r.EndsWith(suffix, StringComparison.Ordinal))
                    .Select(r => r.Substring(0, r.Length - suffix.Length))
                    .ToHashSet();

                return d.Users
                    .Where(u => userIds.Contains(u.Id))
                    .Select(Clone)
                    .ToList();
            });

        // ---------- Seeding ----------

        public void EnsureSeeded(
            IPasswordHasher<ApplicationUser> passwordHasher,
            string adminEmail,
            string adminPassword,
            string defaultUserEmail,
            string defaultUserPassword)
        {
            ArgumentNullException.ThrowIfNull(passwordHasher);

            lock (_lock)
            {
                var modified = false;
                modified |= EnsureRoleSeeded(AdminRole);
                modified |= EnsureRoleSeeded(DefaultUserRole);
                modified |= BackfillNormalizedRoleNames();
                modified |= EnsureUserSeeded(passwordHasher, adminEmail, adminPassword, AdminRole);
                modified |= EnsureUserSeeded(passwordHasher, defaultUserEmail, defaultUserPassword, DefaultUserRole);

                if (modified)
                {
                    SaveInternal();
                }
            }
        }

        private bool EnsureRoleSeeded(string roleName)
        {
            if (_data.Roles.Any(r => r.Name == roleName))
            {
                return false;
            }

            _data.Roles.Add(new IdentityRole(roleName)
            {
                Id = Guid.NewGuid().ToString(),
                NormalizedName = roleName.ToUpperInvariant()
            });
            return true;
        }

        private bool BackfillNormalizedRoleNames()
        {
            var changed = false;
            foreach (var role in _data.Roles)
            {
                if (string.IsNullOrEmpty(role.NormalizedName) && !string.IsNullOrEmpty(role.Name))
                {
                    role.NormalizedName = role.Name.ToUpperInvariant();
                    changed = true;
                }
            }
            return changed;
        }

        private bool EnsureUserSeeded(
            IPasswordHasher<ApplicationUser> passwordHasher,
            string email,
            string password,
            string roleName)
        {
            var normalized = email.ToUpperInvariant();
            var user = _data.Users.FirstOrDefault(u => u.NormalizedEmail == normalized);
            var modified = false;

            if (user == null)
            {
                user = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = email,
                    NormalizedUserName = normalized,
                    Email = email,
                    NormalizedEmail = normalized,
                    EmailConfirmed = true,
                    LockoutEnabled = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                };
                user.PasswordHash = passwordHasher.HashPassword(user, password);
                _data.Users.Add(user);
                modified = true;
            }
            else if (!user.LockoutEnabled)
            {
                user.LockoutEnabled = true;
                modified = true;
            }

            var role = _data.Roles.FirstOrDefault(r => r.Name == roleName);
            if (role != null)
            {
                var key = user.Id + UserRoleSeparator + role.Id;
                if (!_data.UserRoles.Contains(key))
                {
                    _data.UserRoles.Add(key);
                    modified = true;
                }
            }

            return modified;
        }
    }
}
