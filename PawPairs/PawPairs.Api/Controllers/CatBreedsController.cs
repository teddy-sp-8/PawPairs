using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PawPairs.Application.Dtos;
using PawPairs.Application.Interfaces;

namespace PawPairs.Api.Controllers;

[ApiController]
[Route("api/cat-breeds")]
public class CatBreedsController : ControllerBase
{
    private readonly ICatApiClient _cats;

    public CatBreedsController(ICatApiClient cats)
    {
        _cats = cats;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<CatBreedDto>>> Get(
        [FromQuery] string? q = null,
        [FromQuery] string? country = null,
        [FromQuery] string? coatCategory = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 25;

        var raw = await _cats.GetBreedsPageAsync(page, ct);

        var mapped = (raw.Data ?? new List<CatBreedApiItem>())
            .Select(x => new CatBreedDto(
                Breed: x.Breed ?? "",
                Country: x.Country ?? "",
                Origin: x.Origin ?? "",
                Coat: x.Coat ?? "",
                Pattern: x.Pattern ?? "",
                CoatCategory: CategorizeCoat(x.Coat)
            ));

        if (!string.IsNullOrWhiteSpace(q))
            mapped = mapped.Where(x => x.Breed.Contains(q, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(country))
            mapped = mapped.Where(x => x.Country.Contains(country, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(coatCategory))
            mapped = mapped.Where(x => x.CoatCategory.Equals(coatCategory, StringComparison.OrdinalIgnoreCase));

        var list = mapped
            .OrderBy(x => x.Breed)
            .ToList();

        var totalCount = list.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        if (totalPages == 0) totalPages = 1;
        if (page > totalPages) page = totalPages;

        var items = list
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(new PagedResultDto<CatBreedDto>(
            Items: items,
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize,
            TotalPages: totalPages
        ));
    }

    [HttpGet("details")]
    public async Task<ActionResult<CatBreedDto>> Details(
        [FromQuery] string breed,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(breed))
            return BadRequest(new { message = "breed is required" });

        // Search up to 4 pages (catfact says last_page ~4)
        for (var page = 1; page <= 6; page++)
        {
            var raw = await _cats.GetBreedsPageAsync(page, ct);

            var found = (raw.Data ?? new List<CatBreedApiItem>())
                .FirstOrDefault(x => string.Equals(x.Breed, breed, StringComparison.OrdinalIgnoreCase));

            if (found is not null)
            {
                return Ok(new CatBreedDto(
                    Breed: found.Breed ?? "",
                    Country: found.Country ?? "",
                    Origin: found.Origin ?? "",
                    Coat: found.Coat ?? "",
                    Pattern: found.Pattern ?? "",
                    CoatCategory: CategorizeCoat(found.Coat)
                ));
            }

            // if we passed last page, stop
            if (raw.Last_Page > 0 && page >= raw.Last_Page)
                break;
        }

        return NotFound();
    }

    private static string CategorizeCoat(string? coat)
    {
        if (string.IsNullOrWhiteSpace(coat)) return "Unknown";

        var c = coat.ToLowerInvariant();

        if (c.Contains("hairless")) return "Hairless";
        if (c.Contains("semi")) return "Semi-long";
        if (c.Contains("long")) return "Long";
        if (c.Contains("short")) return "Short";
        if (c.Contains("rex")) return "Rex";
        return "Other";
    }
}
