using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ShoesStore.Pages
{
    public class ProductModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string Name { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string Category { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public int Price { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Emoji { get; set; } = "👠";

        [BindProperty(SupportsGet = true)]
        public string Badge { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public int OldPrice { get; set; }

        public string Material { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public List<string> Sizes { get; set; } = new();

        public void OnGet()
        {
            var products = new Dictionary<string, dynamic>
            {
                ["Туфли Elegance"] = new { Material = "Натуральная кожа", Color = "Чёрный", Condition = "Новое", Sizes = new List<string> { "35", "36", "37", "38", "39", "40" } },
                ["Сапоги Winter Comfort"] = new { Material = "Натуральная кожа/Мех", Color = "Коричневый", Condition = "Новое", Sizes = new List<string> { "36", "37", "38", "39", "40", "41" } },
                ["Балетки Soft Step"] = new { Material = "Экокожа", Color = "Бежевый", Condition = "Новое", Sizes = new List<string> { "35", "36", "37", "38", "39" } },
                ["Сандалии Summer Breeze"] = new { Material = "Текстиль/Кожа", Color = "Белый", Condition = "Новое", Sizes = new List<string> { "36", "37", "38", "39", "40" } },
                ["Ботильоны Chelsea"] = new { Material = "Натуральная замша", Color = "Тёмно-коричневый", Condition = "Новое", Sizes = new List<string> { "36", "37", "38", "39", "40", "41" } },
                ["Кроссовки Lady Sport"] = new { Material = "Текстиль/Синтетика", Color = "Белый/Розовый", Condition = "Новое", Sizes = new List<string> { "36", "37", "38", "39", "40", "41" } },
                ["Лоферы Classic"] = new { Material = "Натуральная кожа", Color = "Бордовый", Condition = "Новое", Sizes = new List<string> { "35", "36", "37", "38", "39" } },
                ["Сапоги-трубы Luxe"] = new { Material = "Натуральная кожа", Color = "Чёрный", Condition = "Новое", Sizes = new List<string> { "36", "37", "38", "39", "40" } }
            };

            if (products.ContainsKey(Name))
            {
                var product = products[Name];
                Material = product.Material;
                Color = product.Color;
                Condition = product.Condition;
                Sizes = product.Sizes;
            }
            else
            {
                Material = "Натуральная кожа";
                Color = "Чёрный";
                Condition = "Новое";
                Sizes = new List<string> { "36", "37", "38", "39", "40" };
            }
        }
    }
}
