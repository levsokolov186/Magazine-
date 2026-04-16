using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ShoesStore.Services;
using ShoesStore.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add JSON Database Service
builder.Services.AddSingleton<JsonDatabaseService>(sp =>
    new JsonDatabaseService(Path.Combine(builder.Environment.ContentRootPath, "data.json")));

// Add Identity with JSON storage
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddUserStore<JsonUserStore>()
    .AddRoleStore<JsonRoleStore>()
    .AddDefaultTokenProviders();

// Configure password hasher
builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();

// Configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

// Seed initial data and configure password hasher
using (var scope = app.Services.CreateScope())
{
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<ApplicationUser>>();
    
    var jsonDbService = scope.ServiceProvider.GetRequiredService<JsonDatabaseService>();
    jsonDbService.SetPasswordHasher(passwordHasher);
    jsonDbService.EnsureSeeded();
}

app.Run();