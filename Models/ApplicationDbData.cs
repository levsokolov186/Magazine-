using Microsoft.AspNetCore.Identity;

namespace ShoesStore.Models
{
    public class ApplicationDbData
    {
        public List<Product> Products { get; set; } = new();
        public List<ApplicationUser> Users { get; set; } = new();
        public List<IdentityRole> Roles { get; set; } = new();
        // Stored as "userId|roleId" so renaming a user/role does not break links.
        public List<string> UserRoles { get; set; } = new();
        public int NextProductId { get; set; }
    }
}
