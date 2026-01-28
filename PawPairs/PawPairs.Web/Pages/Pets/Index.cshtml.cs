using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PawPairs.Application.Dtos;
using PawPairs.Domain.Enums;
using PawPairs.Web.Models;

namespace PawPairs.Web.Pages.Pets;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public IndexModel(IHttpClientFactory httpClientFactory)
        => _httpClientFactory = httpClientFactory;

    public List<PetReadDto> Pets { get; private set; } = [];

    public Dictionary<Guid, string> OwnerNameById { get; private set; } = new();

    [BindProperty(SupportsGet = true)] public Species? Species { get; set; }
    [BindProperty(SupportsGet = true)] public EnergyLevel? EnergyLevel { get; set; }
    [BindProperty(SupportsGet = true)] public double? NearLat { get; set; }
    [BindProperty(SupportsGet = true)] public double? NearLng { get; set; }
    [BindProperty(SupportsGet = true)] public double? RadiusKm { get; set; }

    public async Task OnGetAsync()
    {
        var client = _httpClientFactory.CreateClient("PawPairsApi");

        var usersPaged = await client.GetFromJsonAsync<PagedResult<UserReadDto>>("api/users");
        var users = usersPaged?.Items ?? [];

        OwnerNameById = users.ToDictionary(
            u => u.Id,
            u => $"{u.FirstName} {u.LastName}"
        );

        var hasFilters = Species.HasValue || EnergyLevel.HasValue ||
                         NearLat.HasValue || NearLng.HasValue || (RadiusKm.HasValue && RadiusKm.Value > 0);

        if (!hasFilters)
        {
            var petsPaged = await client.GetFromJsonAsync<PagedResult<PetReadDto>>("api/pets");
            Pets = petsPaged?.Items ?? [];
            return;
        }

        var qs = new List<string>();
        if (Species.HasValue) qs.Add($"species={Uri.EscapeDataString(Species.Value.ToString())}");
        if (EnergyLevel.HasValue) qs.Add($"energyLevel={Uri.EscapeDataString(EnergyLevel.Value.ToString())}");
        if (NearLat.HasValue) qs.Add($"nearLat={NearLat.Value}");
        if (NearLng.HasValue) qs.Add($"nearLng={NearLng.Value}");
        if (RadiusKm.HasValue) qs.Add($"radiusKm={RadiusKm.Value}");

        var searchUrl = "api/pets/search" + (qs.Count > 0 ? "?" + string.Join("&", qs) : "");

        try
        {
            var petsPaged = await client.GetFromJsonAsync<PagedResult<PetReadDto>>(searchUrl);
            Pets = petsPaged?.Items ?? [];
        }
        catch
        {
            Pets = await client.GetFromJsonAsync<List<PetReadDto>>(searchUrl) ?? [];
        }
    }

    public record UserReadDto(Guid Id, string FirstName, string LastName, string Email, string City);
}
