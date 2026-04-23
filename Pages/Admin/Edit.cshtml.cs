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
            var product = Db.Products.FirstOrDefault(p => p.Id == id);

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
                Color = product.Color,
                CreatedAt = product.CreatedAt
            };

            SizeEntries = product.Sizes?.Select(s => new SizeEntry
            {
                Id = s.Id,
                Size = s.Size,
                InStock = s.InStock
            }).ToList() ?? new List<SizeEntry>();

            return Page();
        }

        public IActionResult OnPostAsync(string action, decimal? newSize)
        {
            var sizeResult = HandleSizeAction(action, newSize);
            if (sizeResult != null) return sizeResult;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var product = Db.Products.FirstOrDefault(p => p.Id == Product.Id);

            if (product == null)
            {
                return NotFound();
            }

            product.UpdateFrom(Product, SizeEntries);
            Db.SaveProduct(product);

            TempData["SuccessMessage"] = "Товар успешно обновлён";
            return RedirectToPage("Index");
        }
    }
}
