using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PawPairs.Web.Auth;

namespace PawPairs.Web.Pages.Auth;
public record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage
);

public class LoginModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public LoginModel(IHttpClientFactory httpClientFactory)
        => _httpClientFactory = httpClientFactory;

    [BindProperty] public string Email { get; set; } = "";

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            ModelState.AddModelError("", "Email is required.");
            return Page();
        }

        var client = _httpClientFactory.CreateClient("PawPairsApi");
        var page = await client.GetFromJsonAsync<PagedResult<UserReadDto>>("api/users");
        var users = page?.Items ?? [];

        var user = users.FirstOrDefault(u =>
            string.Equals(u.Email, Email, StringComparison.OrdinalIgnoreCase));

        if (user is null)
        {
            ModelState.AddModelError("", "No user with that email. Create one first.");
            return Page();
        }

        SessionAuth.SignIn(HttpContext, user.Id, $"{user.FirstName} {user.LastName}");
        return RedirectToPage("/Dashboard/Index");
    }

    public record UserReadDto(Guid Id, string FirstName, string LastName, string Email, string City);
}