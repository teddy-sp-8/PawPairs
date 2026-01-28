using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PawPairs.Application.Dtos;

namespace PawPairs.Web.Pages.Pets;

public class EditModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public EditModel(IHttpClientFactory httpClientFactory)
        => _httpClientFactory = httpClientFactory;

    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }

    [BindProperty] public string Name { get; set; } = "";
    [BindProperty] public string BreedName { get; set; } = "";
    [BindProperty] public int AgeYears { get; set; }
    [BindProperty] public PawPairs.Domain.Enums.EnergyLevel EnergyLevel { get; set; }
    [BindProperty] public bool IsVaccinated { get; set; }
    [BindProperty] public double Latitude { get; set; }
    [BindProperty] public double Longitude { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var client = _httpClientFactory.CreateClient("PawPairsApi");
        var pet = await client.GetFromJsonAsync<PetReadDto>($"api/pets/{Id}");
        if (pet is null) return RedirectToPage("./Index");

        Name = pet.Name;
        BreedName = pet.BreedName;
        AgeYears = pet.AgeYears;
        EnergyLevel = pet.EnergyLevel;
        IsVaccinated = pet.IsVaccinated;
        Latitude = pet.Latitude;
        Longitude = pet.Longitude;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var client = _httpClientFactory.CreateClient("PawPairsApi");
        var dto = new PetUpdateDto(Name, BreedName, AgeYears, EnergyLevel, IsVaccinated, Latitude, Longitude);

        var resp = await client.PutAsJsonAsync($"api/pets/{Id}", dto);
        if (!resp.IsSuccessStatusCode)
        {
            var msg = await resp.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"API error: {msg}");
            return Page();
        }

        return RedirectToPage("./Index");
    }
}