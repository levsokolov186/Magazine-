using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoesStore.Models;
using ShoesStore.Services;

namespace ShoesStore.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly IProductService _products;

        public CreateModel(IProductService products)
        {
            _products = products;
        }

        [BindProperty]
        public ProductInput Product { get; set; } = new();

        public IActionResult OnGet()
        {
            Product.Sizes = new List<ProductSize>
            {
                new() { Size = 36, InStock = true },
                new() { Size = 37, InStock = true },
                new() { Size = 38, InStock = true },
                new() { Size = 39, InStock = true },
                new() { Size = 40, InStock = true }
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (await _products.ProductNameExistsAsync(Product.Name, cancellationToken: cancellationToken))
            {
                ModelState.AddModelError("Product.Name", "Товар с таким названием уже существует");
                return Page();
            }

            var product = Product.ToEntity();
            await _products.AddProductAsync(product, cancellationToken);

            TempData["SuccessMessage"] = "Товар успешно создан";
            return RedirectToPage("Index");
        }
    }
}
