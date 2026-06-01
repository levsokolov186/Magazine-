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
            // ILIKE is the case-insensitive equality on Postgres; EF.Functions.ILike
            // produces the matching SQL via Npgsql.
            return _db.Products.AnyAsync(
                p => EF.Functions.ILike(p.Name, name)
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
            int id,
            ProductInput input,
            IEnumerable<ProductSize> sizes,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            ArgumentNullException.ThrowIfNull(sizes);

            var product = await _db.Products
                .Include(p => p.Sizes)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            if (product == null) return false;

            // Replace child collection entirely: simpler than diffing
            // and safe because cascade-delete is configured for ProductSize.
            product.Sizes.Clear();
            product.UpdateFrom(input, sizes);

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
