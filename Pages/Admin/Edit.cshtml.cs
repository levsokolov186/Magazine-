using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesStore.Models;
using ShoesStore.Services;

namespace ShoesStore.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class EditModel : AdminProductPageModel
    {
        public EditModel(JsonDatabaseService db) : base(db) { }

        public IActionResult OnGet(int id)
        {
            var product = Db.FindProductById(id);

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

        public IActionResult OnPost(int id, string? action, decimal? newSize)
        {
            // Re-load CreatedAt for sidebar in case we re-render the page.
            var current = Db.FindProductById(id);
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

            // Make sure the bound id always wins so URL tampering can't switch products.
            Product.Id = id;
            current.UpdateFrom(Product, SizeEntries);
            if (!Db.SaveProduct(current))
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Товар успешно обновлён";
            return RedirectToPage("Index");
        }
    }
}
