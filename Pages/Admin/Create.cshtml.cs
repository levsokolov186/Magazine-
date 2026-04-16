using System.ComponentModel.DataAnnotations;
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
        private readonly JsonDatabaseService _db;

        public CreateModel(JsonDatabaseService db)
        {
            _db = db;
        }

        [BindProperty]
        public ProductInput Product { get; set; } = new();

        public List<SizeEntry> SizeEntries { get; set; } = new();

        public class ProductInput
        {
            [Required(ErrorMessage = "Название обязательно")]
            [StringLength(200)]
            [Display(Name = "Название")]
            public string Name { get; set; } = string.Empty;

            [StringLength(500)]
            [Display(Name = "Описание")]
            public string Description { get; set; } = string.Empty;

            [Required(ErrorMessage = "Цена обязательна")]
            [Range(0, 999999, ErrorMessage = "Цена должна быть от 0 до 999999")]
            [Display(Name = "Цена")]
            public decimal Price { get; set; }

            [Range(0, 999999, ErrorMessage = "Старая цена должна быть от 0 до 999999")]
            [Display(Name = "Старая цена")]
            public decimal? OldPrice { get; set; }

            [StringLength(10)]
            [Display(Name = "Эмодзи")]
            public string Emoji { get; set; } = "👠";

            [StringLength(200)]
            [Display(Name = "Категория")]
            public string Category { get; set; } = string.Empty;

            [StringLength(200)]
            [Display(Name = "Материал")]
            public string Material { get; set; } = string.Empty;

            [StringLength(100)]
            [Display(Name = "Цвет")]
            public string Color { get; set; } = string.Empty;
        }

        public class SizeEntry
        {
            public decimal Size { get; set; }
            public bool InStock { get; set; } = true;
        }

        public IActionResult OnGet()
        {
            SizeEntries = new List<SizeEntry>
            {
                new() { Size = 36, InStock = true },
                new() { Size = 37, InStock = true },
                new() { Size = 38, InStock = true },
                new() { Size = 39, InStock = true },
                new() { Size = 40, InStock = true }
            };
            return Page();
        }

        public IActionResult OnPostAsync(string action, decimal? newSize)
        {
            if (action == "add" && newSize.HasValue)
            {
                if (!SizeEntries.Any(s => s.Size == newSize.Value))
                {
                    SizeEntries.Add(new SizeEntry { Size = newSize.Value, InStock = true });
                }
                return Page();
            }

            if (action == "remove" && newSize.HasValue)
            {
                SizeEntries.RemoveAll(s => s.Size == newSize.Value);
                return Page();
            }

            if (action == "toggle" && newSize.HasValue)
            {
                var entry = SizeEntries.FirstOrDefault(s => s.Size == newSize.Value);
                if (entry != null)
                {
                    entry.InStock = !entry.InStock;
                }
                return Page();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var product = new Product
            {
                Name = Product.Name,
                Description = Product.Description,
                Price = Product.Price,
                OldPrice = Product.OldPrice,
                Emoji = Product.Emoji,
                Category = Product.Category,
                Material = Product.Material,
                Color = Product.Color,
                CreatedAt = DateTime.Now,
                Sizes = SizeEntries.Select(s => new ProductSize
                {
                    Size = s.Size,
                    InStock = s.InStock
                }).ToList()
            };

            _db.AddProduct(product);

            TempData["SuccessMessage"] = "Товар успешно создан";
            return RedirectToPage("Index");
        }
    }
}