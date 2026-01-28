using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PawPairs.Web.Pages.DogBreeds;

public class DetailsModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public DetailsModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public DogBreedInfoDto? Breed { get; private set; }

    [BindProperty(SupportsGet = true)]
    public string id { get; set; } = "";   // ✅ matches @page "{id}"

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrWhiteSpace(id))
            return RedirectToPage("./Index");

        var client = _httpClientFactory.CreateClient("PawPairsApi");

        var url = $"api/dog-breeds/{Uri.EscapeDataString(id)}";

        var resp = await client.GetAsync(url);

        if (resp.StatusCode == HttpStatusCode.NotFound)
            return NotFound();

        if (!resp.IsSuccessStatusCode)
        {
            TempData["Error"] = await resp.Content.ReadAsStringAsync();
            return RedirectToPage("./Index");
        }

        Breed = await resp.Content.ReadFromJsonAsync<DogBreedInfoDto>();
        return Page();
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