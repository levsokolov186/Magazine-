using System.ComponentModel.DataAnnotations;

namespace ShoesStore.Models
{
    public class ProductInput
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
        [Range(0.01, 999999, ErrorMessage = "Цена должна быть от 0.01 до 999999")]
        [Display(Name = "Цена")]
        public decimal Price { get; set; }

        [Range(0, 999999, ErrorMessage = "Старая цена должна быть от 0 до 999999")]
        [Display(Name = "Старая цена")]
        public decimal? OldPrice { get; set; }

        [StringLength(10)]
        [Display(Name = "Эмодзи")]
        public string Emoji { get; set; } = "👠";

        [Required(ErrorMessage = "Категория обязательна")]
        [StringLength(200, ErrorMessage = "Категория не должна превышать 200 символов")]
        [Display(Name = "Категория")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Материал обязателен")]
        [StringLength(200, ErrorMessage = "Материал не должен превышать 200 символов")]
        [Display(Name = "Материал")]
        public string Material { get; set; } = string.Empty;

        [Required(ErrorMessage = "Цвет обязателен")]
        [StringLength(100, ErrorMessage = "Цвет не должен превышать 100 символов")]
        [Display(Name = "Цвет")]
        public string Color { get; set; } = string.Empty;
    }
}
