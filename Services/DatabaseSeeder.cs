using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShoesStore.Data;
using ShoesStore.Models;

namespace ShoesStore.Services
{
    /// <summary>
    /// Applies pending EF Core migrations and seeds the two default roles plus
    /// the administrator and default user. Replaces <c>JsonDatabaseService.EnsureSeeded</c>.
    /// </summary>
    public class DatabaseSeeder
    {
        public const string AdminRole = "Admin";
        public const string DefaultUserRole = "User";

        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<DatabaseSeeder> logger)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task SeedAsync(SeedOptions options, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(options);

            // Apply pending migrations on startup. In production you'll usually want to
            // disable this and run `dotnet ef database update` from CI instead.
            await _db.Database.MigrateAsync(cancellationToken);

            await EnsureRoleAsync(AdminRole);
            await EnsureRoleAsync(DefaultUserRole);

            await EnsureUserAsync(options.AdminEmail, options.AdminPassword, AdminRole);
            await EnsureUserAsync(options.DefaultUserEmail, options.DefaultUserPassword, DefaultUserRole);
        }

        private async Task EnsureRoleAsync(string roleName)
        {
            if (await _roleManager.RoleExistsAsync(roleName)) return;

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create role '{roleName}': {FormatErrors(result.Errors)}");
            }

            _logger.LogInformation("Seeded role {Role}", roleName);
        }

        private async Task EnsureUserAsync(string email, string password, string roleName)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    LockoutEnabled = true,
                };

                var create = await _userManager.CreateAsync(user, password);
                if (!create.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to create user '{email}': {FormatErrors(create.Errors)}");
                }

                _logger.LogInformation("Seeded user {Email}", email);
            }
            else if (!user.LockoutEnabled)
            {
                user.LockoutEnabled = true;
                await _userManager.UpdateAsync(user);
            }

            if (!await _userManager.IsInRoleAsync(user, roleName))
            {
                var addToRole = await _userManager.AddToRoleAsync(user, roleName);
                if (!addToRole.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to add user '{email}' to role '{roleName}': {FormatErrors(addToRole.Errors)}");
                }
            }
        }

        private static string FormatErrors(IEnumerable<IdentityError> errors) =>
            string.Join("; ", errors.Select(e => $"{e.Code}: {e.Description}"));

        public sealed record SeedOptions(
            string AdminEmail,
            string AdminPassword,
            string DefaultUserEmail,
            string DefaultUserPassword);
    }
}
