using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesStore.Models;
using ShoesStore.Services;

namespace ShoesStore.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : AdminProductPageModel
    {
        public CreateModel(JsonDatabaseService db) : base(db) { }

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

        public IActionResult OnPost(string? action, decimal? newSize)
        {
            var sizeResult = HandleSizeAction(action, newSize);
            if (sizeResult != null) return sizeResult;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var product = Models.Product.FromInput(Product, SizeEntries);
            Db.AddProduct(product);

            TempData["SuccessMessage"] = "Товар успешно создан";
            return RedirectToPage("Index");
        }
    }
}
