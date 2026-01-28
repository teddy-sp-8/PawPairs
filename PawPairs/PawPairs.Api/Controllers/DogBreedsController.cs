using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PawPairs.Api.Dtos;
using PawPairs.Api.External.DogApi;

namespace PawPairs.Api.Controllers;

[ApiController]
[Route("api/dog-breeds")]
public class DogBreedsController : ControllerBase
{
    private readonly IDogApiClient _dog;

    public DogBreedsController(IDogApiClient dog) => _dog = dog;

    [HttpGet]
    public async Task<ActionResult<List<DogBreedInfoDto>>> GetAll(
        [FromQuery] string? q,
        [FromQuery] string? size,
        [FromQuery] bool? hypo,
        CancellationToken ct)
    {
        var breeds = await _dog.GetBreedsAsync(q, ct);

        if (!string.IsNullOrWhiteSpace(size))
            breeds = breeds.Where(b => b.SizeCategory.Equals(size, StringComparison.OrdinalIgnoreCase)).ToList();

        if (hypo.HasValue)
            breeds = breeds.Where(b => b.Hypoallergenic == hypo.Value).ToList();

        return Ok(breeds);
    }

    [HttpGet("by-name/{name}")]
    public async Task<ActionResult<DogBreedInfoDto>> GetByName(string name, CancellationToken ct)
    {
        var item = await _dog.GetBreedByNameAsync(name, ct);
        return item is null ? NotFound() : Ok(item);
    }
}