using System.ComponentModel.DataAnnotations;

namespace ShoesStore.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Название обязательно")]
        [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
        [Display(Name = "Название")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
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

        [Display(Name = "Создан")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Навигационное свойство
        public List<ProductSize> Sizes { get; set; } = new();
    }
}
