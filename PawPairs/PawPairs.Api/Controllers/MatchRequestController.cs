using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PawPairs.Application.Dtos;
using PawPairs.Application.Interfaces;
using PawPairs.Domain.Enums;

namespace PawPairs.Api.Controllers;

[ApiController]
[Route("api/matchrequests")]
public class MatchRequestsController : ControllerBase
{
    private readonly IMatchRequestService _match;

    public MatchRequestsController(IMatchRequestService match) => _match = match;

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<MatchRequestReadDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;
        
        var pagination = new PaginationDto(page, pageSize);
        var totalCount = await _match.GetCountAsync();
        var items = await _match.GetAllAsync(pagination.Skip, pagination.Take);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        
        return Ok(new PagedResultDto<MatchRequestReadDto>(items, totalCount, page, pageSize, totalPages));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MatchRequestReadDto>> GetById(Guid id)
    {
        var item = await _match.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<MatchRequestReadDto>> Create(MatchRequestCreateDto dto)
    {
        try
        {
            var created = await _match.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
        => await _match.DeleteAsync(id) ? NoContent() : NotFound();

    [HttpPost("{id:guid}/accept")]
    public async Task<IActionResult> Accept(Guid id)
    {
        var existing = await _match.GetByIdAsync(id);
        if (existing is null) return NotFound();

        if (existing.Status != MatchStatus.Pending)
            return BadRequest(new { message = "Only pending requests can be accepted." });

        return await _match.AcceptAsync(id) ? NoContent() : BadRequest();
    }
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id)
    {
        var existing = await _match.GetByIdAsync(id);
        if (existing is null) return NotFound();

        if (existing.Status != MatchStatus.Pending)
            return BadRequest(new { message = "Only pending requests can be rejected." });

        return await _match.RejectAsync(id) ? NoContent() : BadRequest();
    }
    [HttpGet("incoming/{toPetId:guid}")]
    public async Task<ActionResult<List<MatchRequestReadDto>>> GetIncoming(Guid toPetId)
        => await _match.GetIncomingAsync(toPetId);

    [HttpGet("outgoing/{fromPetId:guid}")]
    public async Task<ActionResult<List<MatchRequestReadDto>>> GetOutgoing(Guid fromPetId)
        => await _match.GetOutgoingAsync(fromPetId);
    
    [HttpGet("pending/incoming/{toPetId:guid}")]
    public async Task<ActionResult<List<MatchRequestReadDto>>> GetPendingIncoming(Guid toPetId)
        => await _match.GetPendingIncomingAsync(toPetId);

    [HttpGet("pending/outgoing/{fromPetId:guid}")]
    public async Task<ActionResult<List<MatchRequestReadDto>>> GetPendingOutgoing(Guid fromPetId)
        => await _match.GetPendingOutgoingAsync(fromPetId);


}