using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PawPairs.Application.Dtos;
using PawPairs.Web.Models;

namespace PawPairs.Web.Pages.Matches;

public class CreateModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public CreateModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    [BindProperty] public Guid FromPetId { get; set; }
    [BindProperty] public Guid ToPetId { get; set; }

    public List<PetOption> PetOptions { get; private set; } = [];

    public async Task OnGetAsync()
    {
        var client = _httpClientFactory.CreateClient("PawPairsApi");

        var petsPaged = await client.GetFromJsonAsync<PagedResult<PetReadDto>>("api/pets");
        var pets = petsPaged?.Items ?? [];

        PetOptions = pets
            .OrderBy(p => p.Name)
            .Select(p => new PetOption(p.Id, $"{p.Name} · {p.Species} · {p.BreedName}"))
            .ToList();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (FromPetId == Guid.Empty) ModelState.AddModelError("", "From Pet is required.");
        if (ToPetId == Guid.Empty) ModelState.AddModelError("", "To Pet is required.");
        if (FromPetId != Guid.Empty && FromPetId == ToPetId) ModelState.AddModelError("", "From and To cannot be the same pet.");

        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var client = _httpClientFactory.CreateClient("PawPairsApi");

        var dto = new MatchRequestCreateDto(FromPetId, ToPetId);
        var resp = await client.PostAsJsonAsync("api/matchrequests", dto);

        if (!resp.IsSuccessStatusCode)
        {
            var msg = await resp.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"API error: {msg}");
            await OnGetAsync();
            return Page();
        }

        // redirect to index
        return RedirectToPage("/Matches/Index");
    }

    public record PetOption(Guid Id, string Label);
}
