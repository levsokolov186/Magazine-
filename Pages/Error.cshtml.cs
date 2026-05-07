using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ShoesStore.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ErrorModel : PageModel
    {
        private record ErrorView(string Title, string Heading, string Message);

        private static readonly ErrorView Fallback = new(
            "Ошибка",
            "Произошла ошибка",
            "Что-то пошло не так. Попробуйте обновить страницу или вернуться позже.");

        private static readonly IReadOnlyDictionary<int, ErrorView> KnownErrors =
            new Dictionary<int, ErrorView>
            {
                [404] = new("Страница не найдена",
                            "404 — страница не найдена",
                            "Запрашиваемая страница не существует или была удалена."),
                [403] = new("Доступ запрещён",
                            "403 — доступ запрещён",
                            "У вас нет прав для просмотра этой страницы."),
                [401] = new("Требуется авторизация",
                            "401 — требуется авторизация",
                            "Войдите в учётную запись, чтобы продолжить."),
            };

        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string Title { get; set; } = Fallback.Title;
        public string Heading { get; set; } = Fallback.Heading;
        public string Message { get; set; } = Fallback.Message;

        public void OnGet(int? statusCode = null)
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            var view = statusCode is { } code && KnownErrors.TryGetValue(code, out var known)
                ? known
                : Fallback;

            (Title, Heading, Message) = (view.Title, view.Heading, view.Message);
        }
    }
}
