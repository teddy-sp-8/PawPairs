using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PawPairs.Application.Dtos;

namespace PawPairs.Web.Pages.CatBreeds;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public IndexModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public PagedResultDto<CatBreedDto>? Result { get; private set; }
    public List<CatBreedDto> Breeds => Result?.Items ?? [];

    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true)] public string? Country { get; set; }
    [BindProperty(SupportsGet = true)] public string? CoatCategory { get; set; }

    [BindProperty(SupportsGet = true)] public int Page { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = 25;

    public async Task OnGetAsync()
    {
        var client = _httpClientFactory.CreateClient("PawPairsApi");

        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(Q)) qs.Add($"q={Uri.EscapeDataString(Q)}");
        if (!string.IsNullOrWhiteSpace(Country)) qs.Add($"country={Uri.EscapeDataString(Country)}");
        if (!string.IsNullOrWhiteSpace(CoatCategory)) qs.Add($"coatCategory={Uri.EscapeDataString(CoatCategory)}");

        if (Page < 1) Page = 1;
        if (PageSize < 1 || PageSize > 50) PageSize = 25;

        qs.Add($"page={Page}");
        qs.Add($"pageSize={PageSize}");

        var url = "api/cat-breeds" + (qs.Count > 0 ? "?" + string.Join("&", qs) : "");

        try
        {
            Result = await client.GetFromJsonAsync<PagedResultDto<CatBreedDto>>(url);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
    }
}