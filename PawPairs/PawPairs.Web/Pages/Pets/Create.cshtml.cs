using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PawPairs.Application.Dtos;
using PawPairs.Domain.Enums;
using PawPairs.Web.Models;

namespace PawPairs.Web.Pages.Pets;

public class CreateModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CreateModel(IHttpClientFactory httpClientFactory)
        => _httpClientFactory = httpClientFactory;

    [BindProperty] public Guid OwnerId { get; set; }
    [BindProperty] public string Name { get; set; } = "";
    [BindProperty] public Species Species { get; set; }
    [BindProperty] public string BreedName { get; set; } = "";
    [BindProperty] public int AgeYears { get; set; }
    [BindProperty] public EnergyLevel EnergyLevel { get; set; }
    [BindProperty] public bool IsVaccinated { get; set; }
    [BindProperty] public double Latitude { get; set; }
    [BindProperty] public double Longitude { get; set; }

    public List<UserReadDto> Users { get; private set; } = [];

    public async Task OnGetAsync()
    {
        var client = _httpClientFactory.CreateClient("PawPairsApi");
        var result = await client.GetFromJsonAsync<PagedResult<UserReadDto>>("api/users");
        Users = result?.Items ?? [];
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (OwnerId == Guid.Empty) ModelState.AddModelError("", "Owner is required.");
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var client = _httpClientFactory.CreateClient("PawPairsApi");
        var dto = new PetCreateDto(OwnerId, Name, Species, BreedName, AgeYears, EnergyLevel, IsVaccinated, Latitude, Longitude);

        var resp = await client.PostAsJsonAsync("api/pets", dto);
        if (!resp.IsSuccessStatusCode)
        {
            var msg = await resp.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"API error: {msg}");
            await OnGetAsync();
            return Page();
        }

        return RedirectToPage("./Index");
    }

    public record UserReadDto(Guid Id, string FirstName, string LastName, string Email, string City);
}
