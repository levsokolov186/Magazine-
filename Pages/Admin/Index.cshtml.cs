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
        private readonly IProductService _products;

        public IndexModel(IProductService products)
        {
            _products = products;
        }

        public IList<Product> Products { get; set; } = new List<Product>();

        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            var all = await _products.GetProductsAsync(cancellationToken);
            Products = all.OrderByDescending(p => p.CreatedAt).ToList();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken cancellationToken)
        {
            if (await _products.RemoveProductAsync(id, cancellationToken))
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
