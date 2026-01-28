using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PawPairs.Application.Dtos;

namespace PawPairs.Web.Pages.CatBreeds;

public class DetailsModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public DetailsModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public CatBreedDto? BreedInfo { get; private set; }

    [BindProperty(SupportsGet = true)]
    public string Breed { get; set; } = "";

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrWhiteSpace(Breed))
            return RedirectToPage("./Index");

        var client = _httpClientFactory.CreateClient("PawPairsApi");

        var url = $"api/cat-breeds/details?breed={Uri.EscapeDataString(Breed)}";

        var resp = await client.GetAsync(url);
        if (resp.StatusCode == HttpStatusCode.NotFound)
            return NotFound();

        if (!resp.IsSuccessStatusCode)
        {
            TempData["Error"] = await resp.Content.ReadAsStringAsync();
            return RedirectToPage("./Index");
        }

        BreedInfo = await resp.Content.ReadFromJsonAsync<CatBreedDto>();
        return Page();
    }
}