using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using ShoesStore.Models;

namespace ShoesStore.Services
{
    public class JsonDatabaseService
    {
        private readonly string _dataFilePath;
        private ApplicationDbData _data = null!;
        private IPasswordHasher<ApplicationUser>? _passwordHasher;

        public JsonDatabaseService(string dataFilePath)
        {
            _dataFilePath = dataFilePath;
            LoadData();
        }

        public void SetPasswordHasher(IPasswordHasher<ApplicationUser> passwordHasher)
        {
            _passwordHasher = passwordHasher;
        }

        private void LoadData()
        {
            if (File.Exists(_dataFilePath))
            {
                var json = File.ReadAllText(_dataFilePath);
                _data = JsonSerializer.Deserialize<ApplicationDbData>(json) ?? new ApplicationDbData();
            }
            else
            {
                _data = new ApplicationDbData();
                SaveData();
            }
        }

        private void SaveData()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var json = JsonSerializer.Serialize(_data, options);
            File.WriteAllText(_dataFilePath, json);
        }

        public List<Product> Products
        {
            get => _data.Products;
            set
            {
                _data.Products = value;
                SaveData();
            }
        }

        public List<ProductSize> ProductSizes
        {
            get => _data.ProductSizes;
            set
            {
                _data.ProductSizes = value;
                SaveData();
            }
        }

        public List<ApplicationUser> Users => _data.Users;
        public List<IdentityRole> Roles => _data.Roles;
        public List<string> UserRoles => _data.UserRoles;

        public void AddProduct(Product product)
        {
            if (product.Id == 0)
            {
                product.Id = _data.Products.Any() ? _data.Products.Max(p => p.Id) + 1 : 1;
            }
            _data.Products.Add(product);
            SaveData();
        }

        public void RemoveProduct(Product product)
        {
            _data.Products.Remove(product);
            SaveData();
        }

        public void SaveProduct(Product product)
        {
            var existing = _data.Products.FirstOrDefault(p => p.Id == product.Id);
            if (existing != null)
            {
                _data.Products.Remove(existing);
            }
            _data.Products.Add(product);
            SaveData();
        }

        public void EnsureSeeded()
        {
            if (_passwordHasher == null)
            {
                _passwordHasher = new PasswordHasher<ApplicationUser>();
            }

            if (!_data.Roles.Any(r => r.Name == "Admin"))
            {
                _data.Roles.Add(new IdentityRole("Admin"));
            }
            if (!_data.Roles.Any(r => r.Name == "User"))
            {
                _data.Roles.Add(new IdentityRole("User"));
            }

            if (!_data.Users.Any(u => u.UserName == "admin@stepstyle.ru"))
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@stepstyle.ru",
                    NormalizedUserName = "ADMIN@STEPSTYLE.RU",
                    Email = "admin@stepstyle.ru",
                    NormalizedEmail = "ADMIN@STEPSTYLE.RU",
                    EmailConfirmed = true
                };
                adminUser.PasswordHash = _passwordHasher.HashPassword(adminUser, "Admin123!");
                _data.Users.Add(adminUser);
                _data.UserRoles.Add("admin@stepstyle.ru|Admin");
            }

            if (!_data.Users.Any(u => u.UserName == "user@stepstyle.ru"))
            {
                var regularUser = new ApplicationUser
                {
                    UserName = "user@stepstyle.ru",
                    NormalizedUserName = "USER@STEPSTYLE.RU",
                    Email = "user@stepstyle.ru",
                    NormalizedEmail = "USER@STEPSTYLE.RU",
                    EmailConfirmed = true
                };
                regularUser.PasswordHash = _passwordHasher.HashPassword(regularUser, "User123!");
                _data.Users.Add(regularUser);
            }

            if (_data.Roles.Any() || _data.Users.Any())
            {
                SaveData();
            }
        }
    }

    public class ApplicationDbData
    {
        public List<Product> Products { get; set; } = new();
        public List<ProductSize> ProductSizes { get; set; } = new();
        public List<ApplicationUser> Users { get; set; } = new();
        public List<IdentityRole> Roles { get; set; } = new();
        public List<string> UserRoles { get; set; } = new();
    }
}