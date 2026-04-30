using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoesStore.Models;
using ShoesStore.Services;

namespace ShoesStore.Pages.Admin
{
    public abstract class AdminProductPageModel : PageModel
    {
        private const string ActionAdd = "add";
        private const string ActionRemove = "remove";
        private const string ActionToggle = "toggle";
        private const string ActionSave = "save";

        private const decimal MinSize = 20m;
        private const decimal MaxSize = 50m;

        protected readonly JsonDatabaseService Db;

        protected AdminProductPageModel(JsonDatabaseService db)
        {
            Db = db;
        }

        [BindProperty]
        public ProductInput Product { get; set; } = new();

        [BindProperty]
        public List<ProductSize> SizeEntries { get; set; } = new();

        public DateTime? ProductCreatedAt { get; set; }

        /// <summary>
        /// Returns a Page() result if the request is a size-management action and was handled here.
        /// Returns null if the caller should continue with normal save logic.
        /// </summary>
        protected IActionResult? HandleSizeAction(string? action, decimal? newSize)
        {
            if (string.IsNullOrEmpty(action) || action == ActionSave)
            {
                return null;
            }

            // Anything other than "save" is a size action.
            if (action != ActionAdd && action != ActionRemove && action != ActionToggle)
            {
                return null;
            }

            // Size-management actions must not be blocked by product-field validation.
            ModelState.Clear();

            if (!newSize.HasValue)
            {
                ModelState.AddModelError(string.Empty, "Укажите размер.");
                return Page();
            }

            if (newSize.Value < MinSize || newSize.Value > MaxSize)
            {
                ModelState.AddModelError(string.Empty,
                    $"Размер должен быть от {MinSize} до {MaxSize}.");
                return Page();
            }

            switch (action)
            {
                case ActionAdd:
                    AddSize(newSize.Value);
                    return Page();
                case ActionRemove:
                    RemoveSize(newSize.Value);
                    return Page();
                case ActionToggle:
                    ToggleSize(newSize.Value);
                    return Page();
                default:
                    return null;
            }
        }

        private void AddSize(decimal size)
        {
            if (SizeEntries.Any(s => s.Size == size))
            {
                return;
            }
            SizeEntries.Add(new ProductSize { Size = size, InStock = true });
        }

        private void RemoveSize(decimal size)
        {
            SizeEntries.RemoveAll(s => s.Size == size);
        }

        private void ToggleSize(decimal size)
        {
            var entry = SizeEntries.FirstOrDefault(s => s.Size == size);
            if (entry != null)
            {
                entry.InStock = !entry.InStock;
            }
        }
    }
}
