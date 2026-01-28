using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PawPairs.Application.Dtos;
using PawPairs.Domain.Enums;
using PawPairs.Web.Models;

namespace PawPairs.Web.Pages.Dashboard;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public IndexModel(IHttpClientFactory httpClientFactory)
        => _httpClientFactory = httpClientFactory;

    // Logged-in user (from session)
    public Guid CurrentUserId { get; private set; }

    // Data shown on dashboard
    public List<PetReadDto> MyPets { get; private set; } = [];
    public List<MatchCardVm> PendingIncoming { get; private set; } = [];
    public List<MatchCardVm> PendingOutgoing { get; private set; } = [];
    public List<MatchCardVm> History { get; private set; } = [];

    // Lookups so we can show names not ids
    public Dictionary<Guid, string> PetNameById { get; private set; } = new();
    public Dictionary<Guid, string> OwnerNameById { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        // 1) Get logged in user id from Session
        // Make sure your Login page sets this key.
        var idStr = HttpContext.Session.GetString("UserId");

        if (string.IsNullOrWhiteSpace(idStr) || !Guid.TryParse(idStr, out var userId))
        {
            TempData["Error"] = "You are not logged in. Please login first.";
            return RedirectToPage("/Auth/Login"); // adjust if your login page path differs
        }

        CurrentUserId = userId;

        var client = _httpClientFactory.CreateClient("PawPairsApi");

        // 2) Load Users (for owner display names)
        var usersPaged = await client.GetFromJsonAsync<PagedResult<UserReadDto>>("api/users");
        var users = usersPaged?.Items ?? [];
        OwnerNameById = users.ToDictionary(u => u.Id, u => $"{u.FirstName} {u.LastName}");

        // 3) Load Pets (paged) and build lookups
        var petsPaged = await client.GetFromJsonAsync<PagedResult<PetReadDto>>("api/pets");
        var allPets = petsPaged?.Items ?? [];

        PetNameById = allPets.ToDictionary(p => p.Id, p => p.Name);

        // 4) Filter "my pets"
        MyPets = allPets.Where(p => p.OwnerId == CurrentUserId).ToList();

        // No pets? then dashboard is still valid
        if (MyPets.Count == 0)
            return Page();

        // 5) For each of my pets, load pending incoming/outgoing + history
        var pendingIncoming = new List<MatchRequestReadDto>();
        var pendingOutgoing = new List<MatchRequestReadDto>();
        var history = new List<MatchRequestReadDto>();

        foreach (var pet in MyPets)
        {
            // Pending incoming (TO my pet)
            var pi = await client.GetFromJsonAsync<List<MatchRequestReadDto>>(
                $"api/matchrequests/pending/incoming/{pet.Id}"
            ) ?? [];
            pendingIncoming.AddRange(pi);

            // Pending outgoing (FROM my pet)
            var po = await client.GetFromJsonAsync<List<MatchRequestReadDto>>(
                $"api/matchrequests/pending/outgoing/{pet.Id}"
            ) ?? [];
            pendingOutgoing.AddRange(po);

            // History incoming (all statuses)
            var hi = await client.GetFromJsonAsync<List<MatchRequestReadDto>>(
                $"api/matchrequests/incoming/{pet.Id}"
            ) ?? [];
            history.AddRange(hi);

            // History outgoing (all statuses)
            var ho = await client.GetFromJsonAsync<List<MatchRequestReadDto>>(
                $"api/matchrequests/outgoing/{pet.Id}"
            ) ?? [];
            history.AddRange(ho);
        }

        // 6) Transform to "pretty cards" (names, not ids)
        PendingIncoming = pendingIncoming
            .Where(x => x.Status == MatchStatus.Pending)
            .Select(ToVm)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToList();

        PendingOutgoing = pendingOutgoing
            .Where(x => x.Status == MatchStatus.Pending)
            .Select(ToVm)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToList();

        History = history
            .Where(x => x.Status != MatchStatus.Pending)
            .Select(ToVm)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToList();

        return Page();
    }

    // Post handlers (accept/reject) from dashboard
    public async Task<IActionResult> OnPostAcceptAsync(Guid id)
    {
        var client = _httpClientFactory.CreateClient("PawPairsApi");
        var resp = await client.PostAsync($"api/matchrequests/{id}/accept", null);
        if (!resp.IsSuccessStatusCode)
            TempData["Error"] = await resp.Content.ReadAsStringAsync();

        return RedirectToPage("./Index");
    }

    public async Task<IActionResult> OnPostRejectAsync(Guid id)
    {
        var client = _httpClientFactory.CreateClient("PawPairsApi");
        var resp = await client.PostAsync($"api/matchrequests/{id}/reject", null);
        if (!resp.IsSuccessStatusCode)
            TempData["Error"] = await resp.Content.ReadAsStringAsync();

        return RedirectToPage("./Index");
    }

    private MatchCardVm ToVm(MatchRequestReadDto r)
    {
        var fromPetName = PetNameById.TryGetValue(r.FromPetId, out var fpn) ? fpn : r.FromPetId.ToString();
        var toPetName = PetNameById.TryGetValue(r.ToPetId, out var tpn) ? tpn : r.ToPetId.ToString();

        // owners are on the pets; if your MatchRequestReadDto doesn’t include owner ids,
        // we show just pet names (still MUCH better than ids).
        return new MatchCardVm
        {
            Id = r.Id,
            FromPetId = r.FromPetId,
            ToPetId = r.ToPetId,
            FromPetName = fromPetName,
            ToPetName = toPetName,
            Status = r.Status,
            CreatedAtUtc = r.CreatedAtUtc
        };
    }

    // Local DTOs used by this page
    public record UserReadDto(Guid Id, string FirstName, string LastName, string Email, string City);

    public class MatchCardVm
    {
        public Guid Id { get; set; }
        public Guid FromPetId { get; set; }
        public Guid ToPetId { get; set; }

        public string FromPetName { get; set; } = "";
        public string ToPetName { get; set; } = "";

        public MatchStatus Status { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
