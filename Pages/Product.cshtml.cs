using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoesStore.Models;
using ShoesStore.Services;

namespace ShoesStore.Pages
{
    public class ProductModel : PageModel
    {
        private const string DefaultEmoji = "👠";

        private readonly IProductService _products;

        public ProductModel(IProductService products)
        {
            _products = products;
        }

        public Product Product { get; private set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
        {
            var product = await _products.FindProductByIdAsync(id, cancellationToken);
            if (product is null)
            {
                return NotFound();
            }

            Product = product;
            return Page();
        }
    }
}
