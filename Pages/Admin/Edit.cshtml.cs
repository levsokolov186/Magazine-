using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoesStore.Models;
using ShoesStore.Services;

namespace ShoesStore.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly IProductService _products;

        public EditModel(IProductService products)
        {
            _products = products;
        }

        [BindProperty]
        public ProductInput Product { get; set; } = new();

        public DateTime? ProductCreatedAt { get; set; }

        public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
        {
            var product = await _products.FindProductByIdAsync(id, cancellationToken);

            if (product == null)
            {
                return NotFound();
            }

            Product = ProductInput.FromEntity(product);

            ProductCreatedAt = product.CreatedAt;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id, CancellationToken cancellationToken)
        {
            // Re-load CreatedAt for sidebar in case we re-render the page.
            var current = await _products.FindProductByIdAsync(id, cancellationToken);
            if (current == null)
            {
                return NotFound();
            }
            ProductCreatedAt = current.CreatedAt;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (await _products.ProductNameExistsAsync(Product.Name, excludeId: id, cancellationToken: cancellationToken))
            {
                ModelState.AddModelError("Product.Name", "Товар с таким названием уже существует");
                return Page();
            }

            // Make sure the bound id always wins so URL tampering can't switch products.
            Product.Id = id;
            var productEntity = Product.ToEntity();
            if (!await _products.UpdateProductAsync(productEntity, cancellationToken))
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Товар успешно обновлён";
            return RedirectToPage("Index");
        }
    }
}
