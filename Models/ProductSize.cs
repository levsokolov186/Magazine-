using System.ComponentModel.DataAnnotations;

namespace ShoesStore.Models
{
    public class ProductSize
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Размер обязателен")]
        [Range(20, 50, ErrorMessage = "Размер должен быть от 20 до 50")]
        [Display(Name = "Размер")]
        public decimal Size { get; set; }

        [Display(Name = "В наличии")]
        public bool InStock { get; set; } = true;
    }
}
