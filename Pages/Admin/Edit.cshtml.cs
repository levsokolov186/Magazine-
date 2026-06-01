using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesStore.Models;
using ShoesStore.Services;

namespace ShoesStore.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class EditModel : AdminProductPageModel
    {
        public EditModel(IProductService products) : base(products) { }

        public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
        {
            var product = await Products.FindProductByIdAsync(id, cancellationToken);

            if (product == null)
            {
                return NotFound();
            }

            Product = new ProductInput
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                OldPrice = product.OldPrice,
                Emoji = product.Emoji,
                Category = product.Category,
                Material = product.Material,
                Color = product.Color
            };

            SizeEntries = product.Sizes?.Select(s => new ProductSize
            {
                Size = s.Size,
                InStock = s.InStock
            }).ToList() ?? new List<ProductSize>();

            ProductCreatedAt = product.CreatedAt;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(
            int id,
            string? action,
            decimal? newSize,
            CancellationToken cancellationToken)
        {
            // Re-load CreatedAt for sidebar in case we re-render the page.
            var current = await Products.FindProductByIdAsync(id, cancellationToken);
            if (current == null)
            {
                return NotFound();
            }
            ProductCreatedAt = current.CreatedAt;

            var sizeResult = HandleSizeAction(action, newSize);
            if (sizeResult != null) return sizeResult;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (await Products.ProductNameExistsAsync(Product.Name, excludeId: id, cancellationToken: cancellationToken))
            {
                ModelState.AddModelError("Product.Name", "Товар с таким названием уже существует");
                return Page();
            }

            // Make sure the bound id always wins so URL tampering can't switch products.
            Product.Id = id;
            if (!await Products.UpdateProductAsync(id, Product, SizeEntries, cancellationToken))
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Товар успешно обновлён";
            return RedirectToPage("Index");
        }
    }
}
