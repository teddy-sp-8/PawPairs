using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PawPairs.Application.Dtos;
using PawPairs.Application.Interfaces;
using PawPairs.Domain.Enums;

namespace PawPairs.Api.Controllers;

[ApiController]
[Route("api/pets")]
public class PetsController : ControllerBase
{
    private readonly IPetService _pets;

    public PetsController(IPetService pets) => _pets = pets;

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<PetReadDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;
        
        var pagination = new PaginationDto(page, pageSize);
        var totalCount = await _pets.GetCountAsync();
        var items = await _pets.GetAllAsync(pagination.Skip, pagination.Take);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        
        return Ok(new PagedResultDto<PetReadDto>(items, totalCount, page, pageSize, totalPages));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PetReadDto>> GetById(Guid id)
    {
        var pet = await _pets.GetByIdAsync(id);
        return pet is null ? NotFound() : Ok(pet);
    }

    [HttpPost]
    public async Task<ActionResult<PetReadDto>> Create(PetCreateDto dto)
    {
        try
        {
            var created = await _pets.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, PetUpdateDto dto)
        => await _pets.UpdateAsync(id, dto) ? NoContent() : NotFound();

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
        => await _pets.DeleteAsync(id) ? NoContent() : NotFound();

    // Fancy endpoint: filter/search
    // Example: /api/pets/search?species=Dog&energyLevel=High&nearLat=41.9981&nearLng=21.4254&radiusKm=10
    [HttpGet("search")]
    public async Task<ActionResult<List<PetReadDto>>> Search(
        [FromQuery] Species? species,
        [FromQuery] EnergyLevel? energyLevel,
        [FromQuery] double? nearLat,
        [FromQuery] double? nearLng,
        [FromQuery] double? radiusKm)
        => await _pets.SearchAsync(species, energyLevel, nearLat, nearLng, radiusKm);
}