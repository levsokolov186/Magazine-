using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoesStore.Models;
using ShoesStore.Services;

namespace ShoesStore.Pages.Admin
{
    public abstract class AdminProductPageModel : PageModel
    {
        protected readonly JsonDatabaseService Db;

        protected AdminProductPageModel(JsonDatabaseService db)
        {
            Db = db;
        }

        [BindProperty]
        public ProductInput Product { get; set; } = new();

        public List<SizeEntry> SizeEntries { get; set; } = new();

        protected IActionResult? HandleSizeAction(string action, decimal? newSize)
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

            return null;
        }
    }
}
