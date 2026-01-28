using Microsoft.AspNetCore.Mvc.RazorPages;
using PawPairs.Application.Dtos;
using PawPairs.Web.Models;

namespace PawPairs.Web.Pages.Playdates;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public IndexModel(IHttpClientFactory httpClientFactory)
        => _httpClientFactory = httpClientFactory;

    public List<PlaydateReadDto> Playdates { get; private set; } = [];

    public Dictionary<Guid, string> PetLabelById { get; private set; } = new();

    public async Task OnGetAsync()
    {
        var client = _httpClientFactory.CreateClient("PawPairsApi");

        var petsPaged = await client.GetFromJsonAsync<PagedResult<PetReadDto>>("api/pets");
        var pets = petsPaged?.Items ?? [];

        PetLabelById = pets.ToDictionary(
            p => p.Id,
            p => $"{p.Name} ({p.Species})"
        );

        var result = await client.GetFromJsonAsync<PagedResult<PlaydateReadDto>>("api/playdates");
        Playdates = result?.Items ?? [];
    }
}