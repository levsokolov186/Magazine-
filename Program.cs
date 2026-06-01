using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShoesStore.Data;
using ShoesStore.Models;
using ShoesStore.Services;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder);

var app = builder.Build();

await SeedDatabaseAsync(app);
ConfigurePipeline(app);

app.Run();


static void ConfigureServices(WebApplicationBuilder builder)
{
    builder.Services.AddRazorPages();

    var connectionString =
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
            "Connection string 'DefaultConnection' is not configured. " +
            "Add it to appsettings.json or set the ConnectionStrings__DefaultConnection env var.");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString, npgsql =>
        {
            npgsql.MigrationsHistoryTable("__ef_migrations_history");
            npgsql.EnableRetryOnFailure(maxRetryCount: 3);
        }));

    builder.Services
        .AddIdentity<ApplicationUser, IdentityRole>(ConfigureIdentityOptions)
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    builder.Services.ConfigureApplicationCookie(ConfigureCookieOptions);

    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<DatabaseSeeder>();
}

static void ConfigureIdentityOptions(IdentityOptions options)
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;

    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
}

static void ConfigureCookieOptions(CookieAuthenticationOptions options)
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
}

// Apply pending migrations and seed before serving any requests so the data
// is ready and any seed/migration error fails fast.
static async Task SeedDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    var config = app.Configuration.GetSection("Seed");

    var options = new DatabaseSeeder.SeedOptions(
        AdminEmail: config["AdminEmail"] ?? "admin@stepstyle.ru",
        AdminPassword: config["AdminPassword"] ?? "Admin123!",
        DefaultUserEmail: config["DefaultUserEmail"] ?? "user@stepstyle.ru",
        DefaultUserPassword: config["DefaultUserPassword"] ?? "User123!");

    await seeder.SeedAsync(options);
}

static void ConfigurePipeline(WebApplication app)
{
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }
    app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");

    app.Use(SecurityHeadersMiddleware);

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapRazorPages();
}

// Applied before static files so it covers all responses.
static async Task SecurityHeadersMiddleware(HttpContext context, Func<Task> next)
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "font-src 'self'; " +
        "script-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data:; " +
        "base-uri 'self'; " +
        "form-action 'self'; " +
        "frame-ancestors 'none'";
    await next();
}
