using ShoesStore.Models;

namespace ShoesStore.Services
{
    /// <summary>
    /// Repository-style API for the product catalog.
    /// Replaces the legacy <c>JsonDatabaseService</c> product methods with an
    /// async, EF Core-backed implementation.
    /// </summary>
    public interface IProductService
    {
        Task<IReadOnlyList<Product>> GetProductsAsync(CancellationToken cancellationToken = default);

        Task<Product?> FindProductByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<bool> ProductNameExistsAsync(
            string name,
            int? excludeId = null,
            CancellationToken cancellationToken = default);

        Task<Product> AddProductAsync(Product product, CancellationToken cancellationToken = default);

        Task<bool> RemoveProductAsync(int id, CancellationToken cancellationToken = default);

        Task<bool> UpdateProductAsync(
            Product product,
            CancellationToken cancellationToken = default);
    }
}
