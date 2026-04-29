using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using ShoesStore.Models;

namespace ShoesStore.Services
{
    /// <summary>
    /// Thread-safe JSON-backed store for products, users, roles, and user-role links.
    /// Every read/write goes through <see cref="Read{T}"/> or <see cref="Mutate"/> so
    /// callers don't have to manage locking or remember to persist.
    /// </summary>
    public class JsonDatabaseService
    {
        private const string AdminRole = "Admin";
        private const string DefaultUserRole = "User";
        private const string AdminEmail = "admin@stepstyle.ru";
        private const string AdminPassword = "Admin123!";
        private const string DefaultUserEmail = "user@stepstyle.ru";
        private const string DefaultUserPassword = "User123!";
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

        public void Save() => Mutate(() => { });

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
                _data = JsonSerializer.Deserialize<ApplicationDbData>(json, JsonOptions)
                        ?? new ApplicationDbData();
            }
        }

        private void SaveInternal()
        {
            var json = JsonSerializer.Serialize(_data, JsonOptions);
            File.WriteAllText(_dataFilePath, json);
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

        private void Mutate(Action mutate)
        {
            lock (_lock)
            {
                mutate();
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

        // ---------- Collections (snapshots) ----------

        public IReadOnlyList<Product> Products => Read(d => d.Products.ToList());
        public IReadOnlyList<ApplicationUser> Users => Read(d => d.Users.ToList());
        public IReadOnlyList<IdentityRole> Roles => Read(d => d.Roles.ToList());
        public IReadOnlyList<string> UserRoles => Read(d => d.UserRoles.ToList());

        // ---------- Products ----------

        public Product? FindProductById(int id) =>
            Read(d => d.Products.FirstOrDefault(p => p.Id == id));

        public Product? FindProductByName(string name) =>
            Read(d => d.Products.FirstOrDefault(p => p.Name == name));

        public void AddProduct(Product product) =>
            Mutate(d =>
            {
                if (product.Id == 0)
                {
                    product.Id = NextProductId(d);
                }
                d.Products.Add(product);
            });

        public bool RemoveProduct(int id) =>
            MutateIf(d => d.Products.RemoveAll(p => p.Id == id) > 0);

        public bool SaveProduct(Product product) =>
            MutateIf(d =>
            {
                var index = d.Products.FindIndex(p => p.Id == product.Id);
                if (index < 0)
                {
                    return false;
                }
                if (!ReferenceEquals(d.Products[index], product))
                {
                    d.Products[index] = product;
                }
                return true;
            });

        private static int NextProductId(ApplicationDbData data) =>
            data.Products.Count == 0 ? 1 : data.Products.Max(p => p.Id) + 1;

        // ---------- Users ----------

        public void AddUser(ApplicationUser user) => Mutate(d => d.Users.Add(user));

        public bool RemoveUserById(string id) =>
            MutateIf(d => d.Users.RemoveAll(u => u.Id == id) > 0);

        public ApplicationUser? FindUserById(string id) =>
            Read(d => d.Users.FirstOrDefault(u => u.Id == id));

        public ApplicationUser? FindUserByNormalizedName(string normalizedUserName) =>
            Read(d => d.Users.FirstOrDefault(u => u.NormalizedUserName == normalizedUserName));

        public ApplicationUser? FindUserByNormalizedEmail(string normalizedEmail) =>
            Read(d => d.Users.FirstOrDefault(u => u.NormalizedEmail == normalizedEmail));

        // ---------- Roles ----------

        public void AddRole(IdentityRole role) => Mutate(d => d.Roles.Add(role));

        public bool RemoveRoleById(string id) =>
            MutateIf(d => d.Roles.RemoveAll(r => r.Id == id) > 0);

        public IdentityRole? FindRoleById(string id) =>
            Read(d => d.Roles.FirstOrDefault(r => r.Id == id));

        public IdentityRole? FindRoleByNormalizedName(string normalizedRoleName) =>
            Read(d => d.Roles.FirstOrDefault(r => r.NormalizedName == normalizedRoleName));

        // ---------- User-role links ----------

        public bool AddUserRole(string roleKey) =>
            MutateIf(d =>
            {
                if (d.UserRoles.Contains(roleKey)) return false;
                d.UserRoles.Add(roleKey);
                return true;
            });

        public bool RemoveUserRole(string roleKey) =>
            MutateIf(d => d.UserRoles.Remove(roleKey));

        public bool ContainsUserRole(string roleKey) =>
            Read(d => d.UserRoles.Contains(roleKey));

        public List<string> GetUserRolesByUser(string userName)
        {
            var prefix = userName + UserRoleSeparator;
            return Read(d => d.UserRoles
                .Where(r => r.StartsWith(prefix, StringComparison.Ordinal))
                .Select(r => r.Substring(prefix.Length))
                .ToList());
        }

        public HashSet<string> GetUserNamesInRole(string roleName)
        {
            var suffix = UserRoleSeparator + roleName;
            return Read(d => d.UserRoles
                .Where(r => r.EndsWith(suffix, StringComparison.Ordinal))
                .Select(r => r.Substring(0, r.Length - suffix.Length))
                .ToHashSet());
        }

        // ---------- Seeding ----------

        public void EnsureSeeded(IPasswordHasher<ApplicationUser> passwordHasher)
        {
            ArgumentNullException.ThrowIfNull(passwordHasher);

            lock (_lock)
            {
                var modified = false;
                modified |= EnsureRoleSeeded(AdminRole);
                modified |= EnsureRoleSeeded(DefaultUserRole);
                modified |= BackfillNormalizedRoleNames();
                modified |= EnsureUserSeeded(passwordHasher, AdminEmail, AdminPassword, AdminRole);
                modified |= EnsureUserSeeded(passwordHasher, DefaultUserEmail, DefaultUserPassword, DefaultUserRole);

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
            if (_data.Users.Any(u => u.UserName == email))
            {
                return false;
            }

            var normalized = email.ToUpperInvariant();
            var user = new ApplicationUser
            {
                UserName = email,
                NormalizedUserName = normalized,
                Email = email,
                NormalizedEmail = normalized,
                EmailConfirmed = true
            };
            user.PasswordHash = passwordHasher.HashPassword(user, password);
            _data.Users.Add(user);

            var roleKey = $"{email}{UserRoleSeparator}{roleName}";
            if (!_data.UserRoles.Contains(roleKey))
            {
                _data.UserRoles.Add(roleKey);
            }
            return true;
        }
    }
}
