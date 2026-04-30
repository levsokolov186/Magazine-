using System.ComponentModel.DataAnnotations;

namespace ShoesStore.Models
{
    public class Product
    {
        private const string DefaultEmoji = "👠";
        private const string NewBadge = "Новинка";
        private static readonly TimeSpan NewProductWindow = TimeSpan.FromDays(30);

        public int Id { get; set; }

        [Required(ErrorMessage = "Название обязательно")]
        [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
        [Display(Name = "Название")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
        [Display(Name = "Описание")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Цена обязательна")]
        [Range(0.01, 999999, ErrorMessage = "Цена должна быть от 0.01 до 999999")]
        [Display(Name = "Цена")]
        public decimal Price { get; set; }

        [Range(0, 999999, ErrorMessage = "Старая цена должна быть от 0 до 999999")]
        [Display(Name = "Старая цена")]
        public decimal? OldPrice { get; set; }

        [StringLength(10)]
        [Display(Name = "Эмодзи")]
        public string Emoji { get; set; } = DefaultEmoji;

        [Required(ErrorMessage = "Категория обязательна")]
        [StringLength(200)]
        [Display(Name = "Категория")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Материал обязателен")]
        [StringLength(200)]
        [Display(Name = "Материал")]
        public string Material { get; set; } = string.Empty;

        [Required(ErrorMessage = "Цвет обязателен")]
        [StringLength(100)]
        [Display(Name = "Цвет")]
        public string Color { get; set; } = string.Empty;

        [Display(Name = "Создан")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Изменён")]
        public DateTime UpdatedAt { get; set; }

        public List<ProductSize> Sizes { get; set; } = new();

        public bool HasDiscount =>
            OldPrice.HasValue && OldPrice.Value > 0 && OldPrice.Value > Price;

        public bool IsNew =>
            CreatedAt != default && (DateTime.UtcNow - CreatedAt) <= NewProductWindow;

        public string DiscountBadge
        {
            get
            {
                if (HasDiscount)
                {
                    var pct = (int)Math.Floor((1m - Price / OldPrice!.Value) * 100m);
                    if (pct > 0) return $"-{pct}%";
                }
                return IsNew ? NewBadge : string.Empty;
            }
        }

        public static Product FromInput(ProductInput input, IEnumerable<ProductSize> sizes)
        {
            var product = new Product();
            product.ApplyFrom(input, sizes);
            return product;
        }

        public void UpdateFrom(ProductInput input, IEnumerable<ProductSize> sizes)
        {
            ApplyFrom(input, sizes);
        }

        private void ApplyFrom(ProductInput input, IEnumerable<ProductSize> sizes)
        {
            Name = input.Name;
            Description = input.Description;
            Price = input.Price;
            OldPrice = input.OldPrice;
            Emoji = input.Emoji;
            Category = input.Category;
            Material = input.Material;
            Color = input.Color;
            Sizes = CloneSizes(sizes);

            var now = DateTime.UtcNow;
            UpdatedAt = now;
            if (CreatedAt == default)
            {
                CreatedAt = now;
            }
        }

        public bool ShouldShowBadge => HasDiscount || IsNew;

        private static List<ProductSize> CloneSizes(IEnumerable<ProductSize> sizes) =>
            sizes.Select(s => new ProductSize { Size = s.Size, InStock = s.InStock }).ToList();
    }
}
