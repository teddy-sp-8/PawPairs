using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PawPairs.Web.Pages.Users;

public class EditModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public EditModel(IHttpClientFactory httpClientFactory)
        => _httpClientFactory = httpClientFactory;

    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }

    [BindProperty] public string FirstName { get; set; } = "";
    [BindProperty] public string LastName { get; set; } = "";
    [BindProperty] public string Email { get; set; } = "";
    [BindProperty] public string City { get; set; } = "";

    public async Task<IActionResult> OnGetAsync()
    {
        var client = _httpClientFactory.CreateClient("PawPairsApi");
        var user = await client.GetFromJsonAsync<UserReadDto>($"api/users/{Id}");
        if (user is null) return RedirectToPage("./Index");

        FirstName = user.FirstName;
        LastName = user.LastName;
        Email = user.Email;
        City = user.City;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var client = _httpClientFactory.CreateClient("PawPairsApi");
        var dto = new UserUpdateDto(FirstName, LastName, Email, City);

        var resp = await client.PutAsJsonAsync($"api/users/{Id}", dto);
        if (!resp.IsSuccessStatusCode)
        {
            var msg = await resp.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"API error: {msg}");
            return Page();
        }

        return RedirectToPage("./Index");
    }

    public record UserReadDto(Guid Id, string FirstName, string LastName, string Email, string City);
    public record UserUpdateDto(string FirstName, string LastName, string Email, string City);
}