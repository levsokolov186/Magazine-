using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoesStore.Models;
using ShoesStore.Services;

namespace ShoesStore.Pages.Identity.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; } = "/";

        public class InputModel
        {
            [Required(ErrorMessage = "Email обязателен")]
            [EmailAddress(ErrorMessage = "Неверный формат email")]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Пароль обязателен")]
            [StringLength(100, ErrorMessage = "{0} должен содержать от {2} до {1} символов.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Пароль")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Подтверждение пароля")]
            [Compare("Password", ErrorMessage = "Пароли не совпадают")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        private const string DefaultUserRole = "User";

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = LocalUrlHelper.SanitizeReturnUrl(Url, returnUrl);
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = LocalUrlHelper.SanitizeReturnUrl(Url, returnUrl);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = BuildUser(Input);
            var creation = await _userManager.CreateAsync(user, Input.Password);

            if (!creation.Succeeded)
            {
                AddIdentityErrors(creation);
                return Page();
            }

            await AssignDefaultRoleAsync(user);
            await _signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect(ReturnUrl);
        }

        private static ApplicationUser BuildUser(InputModel input) => new()
        {
            UserName = input.Email,
            Email = input.Email,
            EmailConfirmed = true
        };

        private async Task AssignDefaultRoleAsync(ApplicationUser user)
        {
            if (!await _roleManager.RoleExistsAsync(DefaultUserRole))
            {
                _logger.LogWarning("Роль '{Role}' не существует — пользователь {Email} создан без роли.",
                    DefaultUserRole, user.Email);
                return;
            }

            var roleResult = await _userManager.AddToRoleAsync(user, DefaultUserRole);
            if (!roleResult.Succeeded)
            {
                _logger.LogWarning("Не удалось добавить пользователя {Email} в роль {Role}: {Errors}",
                    user.Email,
                    DefaultUserRole,
                    string.Join("; ", roleResult.Errors.Select(e => e.Description)));
            }
        }

        private void AddIdentityErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}
