using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoesStore.Models;
using ShoesStore.Services;

namespace ShoesStore.Pages
{
    public class ProductModel : PageModel
    {
        private const string DefaultEmoji = "👠";

        private readonly IProductService _products;

        public ProductModel(IProductService products)
        {
            _products = products;
        }

        // Surfaced fields the view actually consumes. We project from the
        // Product entity instead of duplicating it on the page model.
        public int ProductId => _product?.Id ?? 0;
        public string Name => _product?.Name ?? string.Empty;
        public string Category => _product?.Category ?? string.Empty;
        public decimal Price => _product?.Price ?? 0m;
        public string Emoji => string.IsNullOrEmpty(_product?.Emoji) ? DefaultEmoji : _product!.Emoji;
        public string Badge => _product?.DiscountBadge ?? string.Empty;
        public decimal? OldPrice => _product?.OldPrice;
        public string Material => _product?.Material ?? string.Empty;
        public string Color => _product?.Color ?? string.Empty;
        public string Description => _product?.Description ?? string.Empty;
        public IReadOnlyList<ProductSize> Sizes =>
            _product?.Sizes.Where(s => s.InStock).ToList() ?? new List<ProductSize>();
        public bool HasDiscount => _product?.HasDiscount ?? false;

        private Product? _product;

        public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
        {
            _product = await _products.FindProductByIdAsync(id, cancellationToken);
            return _product is null ? NotFound() : Page();
        }
    }
}
