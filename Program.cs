using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using ShoesStore.Services;
using ShoesStore.Models;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder);

var app = builder.Build();

SeedDatabase(app);
ConfigurePipeline(app);

app.Run();


static void ConfigureServices(WebApplicationBuilder builder)
{
    builder.Services.AddRazorPages();

    builder.Services.AddSingleton<JsonDatabaseService>(sp =>
        new JsonDatabaseService(Path.Combine(builder.Environment.ContentRootPath, "data.json")));

    builder.Services
        .AddIdentity<ApplicationUser, IdentityRole>(ConfigureIdentityOptions)
        .AddUserStore<JsonUserStore>()
        .AddRoleStore<JsonRoleStore>()
        .AddDefaultTokenProviders();

    builder.Services.ConfigureApplicationCookie(ConfigureCookieOptions);
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

// Seed before serving any requests so the data is ready and seed errors fail fast.
static void SeedDatabase(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<ApplicationUser>>();
    var jsonDbService = scope.ServiceProvider.GetRequiredService<JsonDatabaseService>();
    var config = app.Configuration.GetSection("Seed");

    jsonDbService.EnsureSeeded(
        passwordHasher,
        config["AdminEmail"] ?? "admin@stepstyle.ru",
        config["AdminPassword"] ?? "Admin123!",
        config["DefaultUserEmail"] ?? "user@stepstyle.ru",
        config["DefaultUserPassword"] ?? "User123!");
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
