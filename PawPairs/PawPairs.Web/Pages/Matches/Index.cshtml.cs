using Microsoft.AspNetCore.Mvc.RazorPages;
using PawPairs.Application.Dtos;
using PawPairs.Web.Models;

namespace PawPairs.Web.Pages.Matches;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public IndexModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public List<MatchRowVm> Requests { get; private set; } = [];

    public async Task OnGetAsync()
    {
        var client = _httpClientFactory.CreateClient("PawPairsApi");

        var petsPaged = await client.GetFromJsonAsync<PagedResult<PetReadDto>>("api/pets");
        var pets = petsPaged?.Items ?? [];

        var petNameById = pets.ToDictionary(
            p => p.Id,
            p => $"{p.Name} · {p.Species}"
        );

        var matchesPaged = await client.GetFromJsonAsync<PagedResult<MatchRequestReadDto>>("api/matchrequests");
        var matches = matchesPaged?.Items ?? [];

        Requests = matches.Select(m => new MatchRowVm
        {
            Id = m.Id,
            FromPetId = m.FromPetId,
            ToPetId = m.ToPetId,
            FromLabel = petNameById.TryGetValue(m.FromPetId, out var f) ? f : "Unknown pet",
            ToLabel = petNameById.TryGetValue(m.ToPetId, out var t) ? t : "Unknown pet",
            Status = m.Status,
            CreatedAtUtc = m.CreatedAtUtc
        }).ToList();
    }

    public class MatchRowVm
    {
        public Guid Id { get; set; }
        public Guid FromPetId { get; set; }
        public Guid ToPetId { get; set; }
        public string FromLabel { get; set; } = "";
        public string ToLabel { get; set; } = "";
        public Domain.Enums.MatchStatus Status { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}