using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoesStore.Models;
using ShoesStore.Services;

namespace ShoesStore.Pages
{
    public class IndexModel : PageModel
    {
        private readonly JsonDatabaseService _dbService;

        public IndexModel(JsonDatabaseService dbService)
        {
            _dbService = dbService;
        }

        public IList<Product> Products { get; set; } = new List<Product>();

        public void OnGet()
        {
            Products = _dbService.Products
                .OrderBy(p => p.Name)
                .ToList();
        }
    }
}
