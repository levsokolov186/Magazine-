using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoesStore.Models;
using ShoesStore.Services;
using System.ComponentModel.DataAnnotations;

namespace ShoesStore.Pages
{
    [IgnoreAntiforgeryToken]
    public class CartModel : PageModel
    {
        private readonly IProductService _products;

        public CartModel(IProductService products)
        {
            _products = products;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostCheckoutAsync([FromBody] CheckoutRequest request, CancellationToken cancellationToken)
        {
            if (request == null || request.Items == null || !request.Items.Any())
            {
                return BadRequest(new { success = false, message = "Корзина пуста" });
            }

            var errors = new List<string>();

            foreach (var item in request.Items)
            {
                var product = await _products.FindProductByIdAsync(item.Id, cancellationToken);
                if (product == null)
                {
                    errors.Add($"Товар '{item.Name}' больше не существует в каталоге.");
                    continue;
                }

                // Invariant Check 1: Price consistency (anti-fraud check)
                if (product.Price != item.Price)
                {
                    errors.Add($"Цена на товар '{product.Name}' изменилась. Актуальная цена: {product.Price} ₽ (было: {item.Price} ₽).");
                }

                // Invariant Check 2: Size and stock availability
                var sizeOption = product.Sizes.FirstOrDefault(s => s.Size == item.Size);
                if (sizeOption == null)
                {
                    errors.Add($"Размер {item.Size} для товара '{product.Name}' недоступен.");
                }
                else if (!sizeOption.InStock)
                {
                    errors.Add($"Размер {item.Size} для товара '{product.Name}' временно отсутствует на складе.");
                }

                // Invariant Check 3: Quantity check
                if (item.Quantity <= 0)
                {
                    errors.Add($"Недопустимое количество ({item.Quantity}) для товара '{product.Name}'.");
                }
            }

            if (errors.Any())
            {
                return new ObjectResult(new { success = false, errors }) { StatusCode = 422 };
            }

            // Return success
            return new JsonResult(new { success = true, message = "Заказ успешно оформлен!" });
        }
    }

    public class CheckoutRequest
    {
        public List<CartItemInput> Items { get; set; } = new();
    }

    public class CartItemInput
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Size { get; set; }
        public int Quantity { get; set; }
    }
}
