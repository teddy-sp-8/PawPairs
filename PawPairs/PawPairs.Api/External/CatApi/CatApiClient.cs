using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using PawPairs.Application.Dtos;
using PawPairs.Application.Interfaces;

namespace PawPairs.Api.External.CatApi;

public class CatApiClient : ICatApiClient
{
    private readonly HttpClient _http;

    public CatApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<CatBreedsApiResponse> GetBreedsPageAsync(int page, CancellationToken ct = default)
    {
        if (page < 1) page = 1;

        var url = $"breeds?page={page}";

        var resp = await _http.GetFromJsonAsync<CatBreedsApiResponse>(url, ct);

        return resp ?? new CatBreedsApiResponse(
            Current_Page: 1,
            Data: new List<CatBreedApiItem>(),
            Last_Page: 1,
            Per_Page: 25,
            Total: 0,
            Next_Page_Url: null,
            Prev_Page_Url: null
        );
    }
}