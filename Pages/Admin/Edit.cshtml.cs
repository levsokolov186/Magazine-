using System.ComponentModel.DataAnnotations;
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
        private readonly JsonDatabaseService _db;

        public EditModel(JsonDatabaseService db)
        {
            _db = db;
        }

        [BindProperty]
        public ProductInput Product { get; set; } = new();

        public List<SizeEntry> SizeEntries { get; set; } = new();

        public class ProductInput
        {
            public int Id { get; set; }

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

            public DateTime CreatedAt { get; set; }
        }

        public class SizeEntry
        {
            public int Id { get; set; }
            public decimal Size { get; set; }
            public bool InStock { get; set; } = true;
        }

        public IActionResult OnGet(int id)
        {
            var product = _db.Products.FirstOrDefault(p => p.Id == id);

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

            var product = _db.Products.FirstOrDefault(p => p.Id == Product.Id);

            if (product == null)
            {
                return NotFound();
            }

            product.Name = Product.Name;
            product.Description = Product.Description;
            product.Price = Product.Price;
            product.OldPrice = Product.OldPrice;
            product.Emoji = Product.Emoji;
            product.Category = Product.Category;
            product.Material = Product.Material;
            product.Color = Product.Color;
            product.Sizes = SizeEntries.Select(s => new ProductSize
            {
                Size = s.Size,
                InStock = s.InStock
            }).ToList();

            _db.SaveProduct(product);

            TempData["SuccessMessage"] = "Товар успешно обновлён";
            return RedirectToPage("Index");
        }
    }
}