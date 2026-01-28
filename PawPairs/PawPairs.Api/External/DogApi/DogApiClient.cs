using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PawPairs.Api.Dtos;

namespace PawPairs.Api.External.DogApi;

public sealed class DogApiOptions
{
    public string BaseUrl { get; set; } = "https://dogapi.dog/api/v2/";
    public int CacheMinutes { get; set; } = 1440;
}

public interface IDogApiClient
{
    Task<List<DogBreedInfoDto>> GetBreedsAsync(string? q, CancellationToken ct);
    Task<DogBreedInfoDto?> GetBreedByNameAsync(string name, CancellationToken ct);
}

public sealed class DogApiClient : IDogApiClient
{
    private const string CacheKeyAllBreeds = "dogapi:breeds:all";
    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly DogApiOptions _opt;

    public DogApiClient(HttpClient http, IMemoryCache cache, IOptions<DogApiOptions> opt)
    {
        _http = http;
        _cache = cache;
        _opt = opt.Value;
    }

    public async Task<List<DogBreedInfoDto>> GetBreedsAsync(string? q, CancellationToken ct)
    {
        var all = await GetAllBreedsCached(ct);

        if (string.IsNullOrWhiteSpace(q))
            return all;

        q = q.Trim();
        return all
            .Where(b => b.Name.Contains(q, StringComparison.OrdinalIgnoreCase))
            .Take(50)
            .ToList();
    }

    public async Task<DogBreedInfoDto?> GetBreedByNameAsync(string name, CancellationToken ct)
    {
        var all = await GetAllBreedsCached(ct);
        return all.FirstOrDefault(b => string.Equals(b.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<List<DogBreedInfoDto>> GetAllBreedsCached(CancellationToken ct)
    {
        if (_cache.TryGetValue(CacheKeyAllBreeds, out List<DogBreedInfoDto>? cached) && cached is not null)
            return cached;

        var resp = await _http.GetFromJsonAsync<DogApiBreedsResponse>("breeds", cancellationToken: ct);
        var data = resp?.Data ?? [];

        var mapped = data
            .Select(x => Map(x.Attributes))
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .OrderBy(x => x.Name)
            .ToList();

        _cache.Set(CacheKeyAllBreeds, mapped, TimeSpan.FromMinutes(_opt.CacheMinutes));
        return mapped;
    }

    private static DogBreedInfoDto Map(DogApiBreedAttributes a)
    {
        var hypo = a.Hypoallergenic ?? false;

        // Heuristic size category using avg male max weight (kg)
        var maleMax = a.MaleWeight?.Max;
        var size = maleMax switch
        {
            null => "Unknown",
            <= 10 => "Small",
            <= 25 => "Medium",
            _ => "Large"
        };

        return new DogBreedInfoDto(
            Name: a.Name,
            Description: a.Description,
            Hypoallergenic: hypo,
            LifeMinYears: a.Life?.Min,
            LifeMaxYears: a.Life?.Max,
            MaleWeightMinKg: a.MaleWeight?.Min,
            MaleWeightMaxKg: a.MaleWeight?.Max,
            FemaleWeightMinKg: a.FemaleWeight?.Min,
            FemaleWeightMaxKg: a.FemaleWeight?.Max,
            SizeCategory: size
        );
    }
}