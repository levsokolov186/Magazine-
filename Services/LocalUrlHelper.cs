using Microsoft.AspNetCore.Mvc;

namespace ShoesStore.Services
{
    /// <summary>
    /// Centralizes the rule for safely redirecting to a caller-supplied returnUrl:
    /// only local URLs are honored, anything else falls back to "/". Keeps Login/Register
    /// from duplicating the same sanitization logic.
    /// </summary>
    public static class LocalUrlHelper
    {
        private const string DefaultReturnUrl = "/";

        public static string SanitizeReturnUrl(IUrlHelper url, string? returnUrl)
        {
            return !string.IsNullOrEmpty(returnUrl) && url.IsLocalUrl(returnUrl)
                ? returnUrl
                : DefaultReturnUrl;
        }
    }
}
