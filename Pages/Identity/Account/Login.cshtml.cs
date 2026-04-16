using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoesStore.Models;

namespace ShoesStore.Pages.Identity.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LoginModel(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string ReturnUrl { get; set; } = "/";

        public class InputModel
        {
            [Required(ErrorMessage = "Email обязателен")]
            [EmailAddress(ErrorMessage = "Неверный формат email")]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Пароль обязателен")]
            [DataType(DataType.Password)]
            [Display(Name = "Пароль")]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Запомнить меня")]
            public bool RememberMe { get; set; }
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? "/";
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? "/";

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    Input.Email, 
                    Input.Password, 
                    Input.RememberMe, 
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return LocalRedirect(ReturnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Неверный email или пароль");
                }
            }

            return Page();
        }
    }
}
