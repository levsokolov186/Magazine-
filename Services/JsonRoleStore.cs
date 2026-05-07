using Microsoft.AspNetCore.Identity;

namespace ShoesStore.Services
{
    public class JsonRoleStore : IRoleStore<IdentityRole>
    {
        private readonly JsonDatabaseService _db;

        public JsonRoleStore(JsonDatabaseService db)
        {
            _db = db;
        }

        public Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role.NormalizedName != null && _db.FindRoleByNormalizedName(role.NormalizedName) != null)
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError
                {
                    Code = "DuplicateRoleName",
                    Description = "Роль с таким именем уже существует."
                }));
            }
            if (string.IsNullOrEmpty(role.Id))
            {
                role.Id = Guid.NewGuid().ToString();
            }
            _db.AddRole(role);
            return Task.FromResult(IdentityResult.Success);
        }

        public Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_db.SaveRole(role)
                ? IdentityResult.Success
                : IdentityResult.Failed(new IdentityError
                {
                    Code = "RoleNotFound",
                    Description = "Роль не найдена."
                }));
        }

        public Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _db.RemoveRoleById(role.Id);
            return Task.FromResult(IdentityResult.Success);
        }

        public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(role.Id);
        }

        public Task<string?> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(role.Name);
        }

        public Task SetRoleNameAsync(IdentityRole role, string? roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            role.Name = roleName;
            return Task.CompletedTask;
        }

        public Task<string?> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(role.NormalizedName);
        }

        public Task SetNormalizedRoleNameAsync(IdentityRole role, string? normalizedName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            role.NormalizedName = normalizedName;
            return Task.CompletedTask;
        }

        public Task<IdentityRole?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_db.FindRoleById(roleId));
        }

        public Task<IdentityRole?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_db.FindRoleByNormalizedName(normalizedRoleName));
        }

        public void Dispose()
        {
            // No unmanaged resources to release.
        }
    }
}
