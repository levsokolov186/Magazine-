using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ShoesStore.Pages.Identity.Account
{
    public class AccessDeniedModel : PageModel
    {
        public void OnGet()
        {
            Response.StatusCode = 403;
        }
    }
}
