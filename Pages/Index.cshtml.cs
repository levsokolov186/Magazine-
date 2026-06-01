using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoesStore.Models;
using ShoesStore.Services;

namespace ShoesStore.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IProductService _products;

        public IndexModel(IProductService products)
        {
            _products = products;
        }

        public IList<Product> Products { get; set; } = new List<Product>();

        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            var all = await _products.GetProductsAsync(cancellationToken);
            Products = all.OrderBy(p => p.Name).ToList();
        }
    }
}
