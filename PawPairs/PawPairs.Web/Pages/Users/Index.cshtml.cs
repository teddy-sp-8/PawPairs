using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PawPairs.Web.Models;

namespace PawPairs.Web.Pages.Users;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public IndexModel(IHttpClientFactory httpClientFactory)
        => _httpClientFactory = httpClientFactory;

    public List<UserReadDto> Users { get; private set; } = [];

    public async Task OnGetAsync()
    {
        var client = _httpClientFactory.CreateClient("PawPairsApi");

        var result = await client.GetFromJsonAsync<PagedResult<UserReadDto>>("api/users");
        Users = result?.Items ?? [];
        
    }

    public record UserReadDto(Guid Id, string FirstName, string LastName, string Email, string City);
}