using Microsoft.EntityFrameworkCore;
using ShoesStore.Data;
using ShoesStore.Models;

namespace ShoesStore.Services
{
    /// <summary>
    /// EF Core / PostgreSQL implementation of <see cref="IProductService"/>.
    /// Scoped per-request — the same lifetime as <see cref="ApplicationDbContext"/>.
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _db;

        public ProductService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<Product>> GetProductsAsync(CancellationToken cancellationToken = default)
        {
            // AsNoTracking — read-only listing, avoids change-tracker overhead.
            return await _db.Products
                .AsNoTracking()
                .Include(p => p.Sizes)
                .OrderBy(p => p.Name)
                .ToListAsync(cancellationToken);
        }

        public Task<Product?> FindProductByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _db.Products
                .AsNoTracking()
                .Include(p => p.Sizes)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public Task<bool> ProductNameExistsAsync(
            string name,
            int? excludeId = null,
            CancellationToken cancellationToken = default)
        {
            // MySQL/MariaDB text comparison is case-insensitive by default under standard collations.
            return _db.Products.AnyAsync(
                p => p.Name == name
                     && (!excludeId.HasValue || p.Id != excludeId.Value),
                cancellationToken);
        }

        public async Task<Product> AddProductAsync(Product product, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(product);

            var now = DateTime.UtcNow;
            if (product.CreatedAt == default) product.CreatedAt = now;
            product.UpdatedAt = now;

            _db.Products.Add(product);
            await _db.SaveChangesAsync(cancellationToken);
            return product;
        }

        public async Task<bool> RemoveProductAsync(int id, CancellationToken cancellationToken = default)
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            if (product == null) return false;

            _db.Products.Remove(product);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateProductAsync(
            Product product,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(product);

            var existing = await _db.Products
                .Include(p => p.Sizes)
                .FirstOrDefaultAsync(p => p.Id == product.Id, cancellationToken);
            if (existing == null) return false;

            existing.Name = product.Name;
            existing.Description = product.Description;
            existing.Price = product.Price;
            existing.OldPrice = product.OldPrice;
            existing.Emoji = product.Emoji;
            existing.Category = product.Category;
            existing.Material = product.Material;
            existing.Color = product.Color;

            existing.Sizes.Clear();
            foreach (var size in product.Sizes)
            {
                existing.Sizes.Add(new ProductSize
                {
                    Size = size.Size,
                    InStock = size.InStock
                });
            }

            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
