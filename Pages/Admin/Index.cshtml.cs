using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoesStore.Models;
using ShoesStore.Services;

namespace ShoesStore.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly JsonDatabaseService _db;

        public IndexModel(JsonDatabaseService db)
        {
            _db = db;
        }

        public IList<Product> Products { get; set; } = new List<Product>();

        public void OnGet()
        {
            Products = _db.Products.OrderByDescending(p => p.CreatedAt).ToList();
        }

        public IActionResult OnPostDelete(int id)
        {
            var product = _db.Products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                _db.RemoveProduct(product);
                TempData["SuccessMessage"] = "Товар успешно удалён";
            }
            return RedirectToPage();
        }
    }
}