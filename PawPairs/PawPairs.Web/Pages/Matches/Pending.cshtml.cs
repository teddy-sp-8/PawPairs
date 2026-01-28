using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PawPairs.Application.Dtos;
using PawPairs.Web.Models;

namespace PawPairs.Web.Pages.Matches;

public class PendingModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public PendingModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    [BindProperty(SupportsGet = true)]
    public Guid ToPetId { get; set; }

    public List<PetOption> PetOptions { get; private set; } = [];
    public List<PendingVm> Pending { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        var client = _httpClientFactory.CreateClient("PawPairsApi");

        var usersPaged = await client.GetFromJsonAsync<PagedResult<UserReadDto>>("api/users");
        var users = usersPaged?.Items ?? [];
        var userNameById = users.ToDictionary(u => u.Id, u => $"{u.FirstName} {u.LastName}");

        var petsPaged = await client.GetFromJsonAsync<PagedResult<PetReadDto>>("api/pets");
        var pets = petsPaged?.Items ?? [];

        var petById = pets.ToDictionary(p => p.Id, p => p);
        PetOptions = pets
            .OrderBy(p => p.Name)
            .Select(p =>
            {
                var ownerName = userNameById.TryGetValue(p.OwnerId, out var o) ? o : "Unknown owner";
                return new PetOption(p.Id, $"{p.Name} · {p.Species} · {ownerName}");
            })
            .ToList();

        if (ToPetId == Guid.Empty)
        {
            Pending = [];
            return Page();
        }

        var url = $"api/matchrequests/pending/incoming/{ToPetId}";
        var raw = await client.GetFromJsonAsync<List<MatchRequestReadDto>>(url) ?? [];

        Pending = raw.Select(r =>
        {
            petById.TryGetValue(r.FromPetId, out var fromPet);
            petById.TryGetValue(r.ToPetId, out var toPet);

            var fromOwner = fromPet != null && userNameById.TryGetValue(fromPet.OwnerId, out var fo) ? fo : "Unknown owner";
            var toOwner = toPet != null && userNameById.TryGetValue(toPet.OwnerId, out var to) ? to : "Unknown owner";

            return new PendingVm
            {
                Id = r.Id,
                CreatedAtUtc = r.CreatedAtUtc,
                FromPetName = fromPet?.Name ?? "Unknown pet",
                ToPetName = toPet?.Name ?? "Unknown pet",
                FromOwnerName = fromOwner,
                ToOwnerName = toOwner
            };
        }).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAcceptAsync(Guid id)
    {
        var client = _httpClientFactory.CreateClient("PawPairsApi");
        var resp = await client.PostAsync($"api/matchrequests/{id}/accept", null);

        if (!resp.IsSuccessStatusCode)
            TempData["Error"] = await resp.Content.ReadAsStringAsync();

        return RedirectToPage("./Pending", new { ToPetId });
    }

    public async Task<IActionResult> OnPostRejectAsync(Guid id)
    {
        var client = _httpClientFactory.CreateClient("PawPairsApi");
        var resp = await client.PostAsync($"api/matchrequests/{id}/reject", null);

        if (!resp.IsSuccessStatusCode)
            TempData["Error"] = await resp.Content.ReadAsStringAsync();

        return RedirectToPage("./Pending", new { ToPetId });
    }

    public record PetOption(Guid Id, string Label);
    public record UserReadDto(Guid Id, string FirstName, string LastName, string Email, string City);

    public class PendingVm
    {
        public Guid Id { get; set; }
        public string FromPetName { get; set; } = "";
        public string ToPetName { get; set; } = "";
        public string FromOwnerName { get; set; } = "";
        public string ToOwnerName { get; set; } = "";
        public DateTime CreatedAtUtc { get; set; }
    }
}
