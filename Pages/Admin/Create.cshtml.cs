using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesStore.Models;
using ShoesStore.Services;

namespace ShoesStore.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : AdminProductPageModel
    {
        public CreateModel(IProductService products) : base(products) { }

        public IActionResult OnGet()
        {
            SizeEntries = new List<ProductSize>
            {
                new() { Size = 36, InStock = true },
                new() { Size = 37, InStock = true },
                new() { Size = 38, InStock = true },
                new() { Size = 39, InStock = true },
                new() { Size = 40, InStock = true }
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(
            string? action,
            decimal? newSize,
            CancellationToken cancellationToken)
        {
            var sizeResult = HandleSizeAction(action, newSize);
            if (sizeResult != null) return sizeResult;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (await Products.ProductNameExistsAsync(Product.Name, cancellationToken: cancellationToken))
            {
                ModelState.AddModelError("Product.Name", "Товар с таким названием уже существует");
                return Page();
            }

            var product = Models.Product.FromInput(Product, SizeEntries);
            await Products.AddProductAsync(product, cancellationToken);

            TempData["SuccessMessage"] = "Товар успешно создан";
            return RedirectToPage("Index");
        }
    }
}
