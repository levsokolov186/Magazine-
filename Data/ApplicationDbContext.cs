using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShoesStore.Models;

namespace ShoesStore.Data
{
    /// <summary>
    /// EF Core context that hosts ASP.NET Core Identity tables plus the product catalog.
    /// Switched from the file-based <c>JsonDatabaseService</c> to PostgreSQL via Npgsql.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductSize> ProductSizes => Set<ProductSize>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            ConfigureProduct(builder);
            ConfigureProductSize(builder);
            RenameIdentityTables(builder);
        }

        private static void ConfigureProduct(ModelBuilder builder)
        {
            builder.Entity<Product>(entity =>
            {
                entity.ToTable("products");

                entity.HasKey(p => p.Id);

                entity.Property(p => p.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasIndex(p => p.Name)
                    .IsUnique()
                    .HasDatabaseName("ix_products_name");

                entity.Property(p => p.Description)
                    .HasMaxLength(500)
                    .HasDefaultValue(string.Empty);

                entity.Property(p => p.Price)
                    .HasColumnType("numeric(12,2)");

                entity.Property(p => p.OldPrice)
                    .HasColumnType("numeric(12,2)");

                entity.Property(p => p.Emoji)
                    .HasMaxLength(10);

                entity.Property(p => p.Category)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(p => p.Material)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(p => p.Color)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(p => p.CreatedAt);

                entity.Property(p => p.UpdatedAt);

                entity.HasMany(p => p.Sizes)
                    .WithOne()
                    .HasForeignKey("ProductId")
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureProductSize(ModelBuilder builder)
        {
            builder.Entity<ProductSize>(entity =>
            {
                entity.ToTable("product_sizes");

                // Shadow primary key keeps the POCO clean while letting EF identify rows.
                entity.Property<int>("Id")
                    .ValueGeneratedOnAdd();
                entity.HasKey("Id");

                entity.Property<int>("ProductId");

                entity.Property(s => s.Size)
                    .HasColumnType("numeric(4,1)");

                entity.Property(s => s.InStock)
                    .HasDefaultValue(true);

                entity.HasIndex("ProductId", nameof(ProductSize.Size))
                    .IsUnique()
                    .HasDatabaseName("ix_product_sizes_product_size");
            });
        }

        // Map the default AspNet* tables to snake_case names so the schema lines up
        // with the products tables. Helpful when poking around with psql.
        private static void RenameIdentityTables(ModelBuilder builder)
        {
            builder.Entity<ApplicationUser>().ToTable("users");
            builder.Entity<IdentityRole>().ToTable("roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("user_roles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("user_claims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("user_logins");
            builder.Entity<IdentityUserToken<string>>().ToTable("user_tokens");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("role_claims");
        }
    }
}
