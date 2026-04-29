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
            if (_db.RemoveProduct(id))
            {
                TempData["SuccessMessage"] = "Товар успешно удалён";
            }
            else
            {
                TempData["ErrorMessage"] = "Товар не найден";
            }
            return RedirectToPage();
        }
    }
}
