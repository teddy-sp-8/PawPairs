using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PawPairs.Web.Pages.Users;

public class CreateModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CreateModel(IHttpClientFactory httpClientFactory)
        => _httpClientFactory = httpClientFactory;

    [BindProperty] public string FirstName { get; set; } = "";
    [BindProperty] public string LastName { get; set; } = "";
    [BindProperty] public string Email { get; set; } = "";
    [BindProperty] public string City { get; set; } = "";

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(FirstName)) ModelState.AddModelError("", "First name is required.");
        if (string.IsNullOrWhiteSpace(LastName)) ModelState.AddModelError("", "Last name is required.");
        if (string.IsNullOrWhiteSpace(Email)) ModelState.AddModelError("", "Email is required.");

        if (!ModelState.IsValid) return Page();

        var client = _httpClientFactory.CreateClient("PawPairsApi");
        var dto = new UserCreateDto(FirstName, LastName, Email, City);

        var resp = await client.PostAsJsonAsync("api/users", dto);
        if (!resp.IsSuccessStatusCode)
        {
            var msg = await resp.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"API error: {msg}");
            return Page();
        }

        return RedirectToPage("./Index");
    }

    public record UserCreateDto(string FirstName, string LastName, string Email, string City);
}