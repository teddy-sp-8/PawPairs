using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PawPairs.Web.Pages.DogBreeds;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public IndexModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public List<DogBreedInfoDto> Breeds { get; private set; } = [];

    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true)] public string? Size { get; set; }
    [BindProperty(SupportsGet = true)] public bool? Hypo { get; set; } 

    public async Task OnGetAsync()
    {
        var client = _httpClientFactory.CreateClient("PawPairsApi");

        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(Q)) qs.Add($"q={Uri.EscapeDataString(Q)}");
        if (!string.IsNullOrWhiteSpace(Size)) qs.Add($"size={Uri.EscapeDataString(Size)}");
        if (Hypo.HasValue) qs.Add($"hypo={Hypo.Value.ToString().ToLowerInvariant()}");

        var url = "api/dog-breeds" + (qs.Count > 0 ? "?" + string.Join("&", qs) : "");

        try
        {
            Breeds = await client.GetFromJsonAsync<List<DogBreedInfoDto>>(url) ?? [];
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            Breeds = [];
        }
    }

    public record DogBreedInfoDto(
        string Id,
        string Name,
        string? Description,
        bool Hypoallergenic,
        int? LifeMinYears,
        int? LifeMaxYears,
        int? MaleWeightMinKg,
        int? MaleWeightMaxKg,
        int? FemaleWeightMinKg,
        int? FemaleWeightMaxKg,
        string SizeCategory
    );
}