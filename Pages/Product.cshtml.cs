using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoesStore.Models;
using ShoesStore.Services;

namespace ShoesStore.Pages
{
    public class ProductModel : PageModel
    {
        private readonly JsonDatabaseService _db;

        public ProductModel(JsonDatabaseService db)
        {
            _db = db;
        }

        [BindProperty(SupportsGet = true)]
        public string Name { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string Category { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public decimal Price { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Emoji { get; set; } = "👠";

        [BindProperty(SupportsGet = true)]
        public string Badge { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public decimal OldPrice { get; set; }

        public string Material { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Condition { get; set; } = "Новое";
        public List<ProductSize> Sizes { get; set; } = new();

        public void OnGet()
        {
            var product = _db.Products.FirstOrDefault(p => p.Name == Name);

            if (product != null)
            {
                Material = product.Material;
                Color = product.Color;
                Condition = "Новое";
                Sizes = product.Sizes?.Where(s => s.InStock).ToList() ?? new List<ProductSize>();
                Price = product.Price;
                Emoji = product.Emoji;
                Category = product.Category;
                OldPrice = product.OldPrice ?? 0;
            }
            else
            {
                Material = "Натуральная кожа";
                Color = "Чёрный";
                Condition = "Новое";
                Sizes = new List<ProductSize>
                {
                    new() { Size = 36, InStock = true },
                    new() { Size = 37, InStock = true },
                    new() { Size = 38, InStock = true },
                    new() { Size = 39, InStock = true },
                    new() { Size = 40, InStock = true }
                };
            }
        }
    }
}