using Microsoft.AspNetCore.Identity;

namespace ShoesStore.Models
{
    public class ApplicationDbData
    {
        public List<Product> Products { get; set; } = new();
        public List<ApplicationUser> Users { get; set; } = new();
        public List<IdentityRole> Roles { get; set; } = new();
        public List<string> UserRoles { get; set; } = new();
    }
}
