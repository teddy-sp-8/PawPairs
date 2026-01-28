using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PawPairs.Application.Dtos;
using PawPairs.Application.Interfaces;

namespace PawPairs.Api.Controllers;

[ApiController]
[Route("api/playdates")]
public class PlaydatesController : ControllerBase
{
    private readonly IPlaydateService _playdates;

    public PlaydatesController(IPlaydateService playdates) => _playdates = playdates;

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<PlaydateReadDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;
        
        var pagination = new PaginationDto(page, pageSize);
        var totalCount = await _playdates.GetCountAsync();
        var items = await _playdates.GetAllAsync(pagination.Skip, pagination.Take);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        
        return Ok(new PagedResultDto<PlaydateReadDto>(items, totalCount, page, pageSize, totalPages));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PlaydateReadDto>> GetById(Guid id)
    {
        var playdate = await _playdates.GetByIdAsync(id);
        return playdate is null ? NotFound() : Ok(playdate);
    }

    [HttpPost]
    public async Task<ActionResult<PlaydateReadDto>> Create(PlaydateCreateDto dto)
    {
        try
        {
            var created = await _playdates.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, PlaydateUpdateDto dto)
        => await _playdates.UpdateAsync(id, dto) ? NoContent() : NotFound();

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
        => await _playdates.DeleteAsync(id) ? NoContent() : NotFound();
}