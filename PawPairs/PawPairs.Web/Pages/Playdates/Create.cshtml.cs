using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PawPairs.Application.Dtos;
using PawPairs.Web.Models;

namespace PawPairs.Web.Pages.Playdates;

public class CreateModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CreateModel(IHttpClientFactory httpClientFactory)
        => _httpClientFactory = httpClientFactory;

    public List<PetReadDto> Pets { get; private set; } = [];

    [BindProperty] public Guid PetAId { get; set; }
    [BindProperty] public Guid PetBId { get; set; }
    [BindProperty] public DateTime ScheduledAtUtc { get; set; } = DateTime.UtcNow.AddDays(1);
    [BindProperty] public string LocationName { get; set; } = "";
    [BindProperty] public string? Notes { get; set; }

    public async Task OnGetAsync()
    {
        var client = _httpClientFactory.CreateClient("PawPairsApi");
        var result = await client.GetFromJsonAsync<PagedResult<PetReadDto>>("api/pets");
        Pets = result?.Items ?? [];
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (PetAId == Guid.Empty || PetBId == Guid.Empty)
            ModelState.AddModelError("", "Both pets are required.");
        if (PetAId == PetBId)
            ModelState.AddModelError("", "PetA and PetB must be different.");
        if (string.IsNullOrWhiteSpace(LocationName))
            ModelState.AddModelError("", "Location is required.");

        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var client = _httpClientFactory.CreateClient("PawPairsApi");
        var dto = new PlaydateCreateDto(PetAId, PetBId, ScheduledAtUtc, LocationName, Notes);

        var resp = await client.PostAsJsonAsync("api/playdates", dto);
        if (!resp.IsSuccessStatusCode)
        {
            var msg = await resp.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"API error: {msg}");
            await OnGetAsync();
            return Page();
        }

        return RedirectToPage("./Index");
    }
}
