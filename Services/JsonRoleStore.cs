using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using ShoesStore.Models;

namespace ShoesStore.Services
{
    public class JsonRoleStore :
        IRoleStore<IdentityRole>,
        IRoleClaimStore<IdentityRole>
    {
        private readonly JsonDatabaseService _db;

        public JsonRoleStore(JsonDatabaseService db)
        {
            _db = db;
        }

        public async Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var roles = _db.Roles;
            role.Id = Guid.NewGuid().ToString();
            roles.Add(role);
            await Task.CompletedTask;
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.CompletedTask;
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var roles = _db.Roles;
            roles.Remove(role);
            await Task.CompletedTask;
            return IdentityResult.Success;
        }

        public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Id);
        }

        public Task<string?> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        {
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
            var role = _db.Roles.FirstOrDefault(r => r.Id == roleId);
            return Task.FromResult(role);
        }

        public Task<IdentityRole?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var role = _db.Roles.FirstOrDefault(r => r.NormalizedName == normalizedRoleName);
            return Task.FromResult(role);
        }

        public Task<IList<Claim>> GetClaimsAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult<IList<Claim>>(new List<Claim>());
        }

        public Task AddClaimAsync(IdentityRole role, Claim claim, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task RemoveClaimAsync(IdentityRole role, Claim claim, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}