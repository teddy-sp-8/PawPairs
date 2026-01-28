using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PawPairs.Web.Pages.Users;

public class DeleteModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DeleteModel(IHttpClientFactory httpClientFactory)
        => _httpClientFactory = httpClientFactory;

    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }

    public UserReadDto? UserToDelete { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var client = _httpClientFactory.CreateClient("PawPairsApi");
        UserToDelete = await client.GetFromJsonAsync<UserReadDto>($"api/users/{Id}");
        if (UserToDelete is null) return RedirectToPage("./Index");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var client = _httpClientFactory.CreateClient("PawPairsApi");
        var resp = await client.DeleteAsync($"api/users/{Id}");
        if (!resp.IsSuccessStatusCode)
        {
            TempData["Error"] = await resp.Content.ReadAsStringAsync();
            return RedirectToPage("./Index");
        }

        return RedirectToPage("./Index");
    }

    public record UserReadDto(Guid Id, string FirstName, string LastName, string Email, string City);
}