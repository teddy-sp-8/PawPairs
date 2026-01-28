using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PawPairs.Application.Dtos;
using PawPairs.Application.Interfaces;

namespace PawPairs.Api.Controllers;

[ApiController]
[Route("/api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _users;

    public UsersController(IUserService users) => _users = users;

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<UserReadDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;
        
        var pagination = new PaginationDto(page, pageSize);
        var totalCount = await _users.GetCountAsync();
        var items = await _users.GetAllAsync(pagination.Skip, pagination.Take);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        
        return Ok(new PagedResultDto<UserReadDto>(items, totalCount, page, pageSize, totalPages));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserReadDto>> GetById(Guid id)
    {
        var user = await _users.GetByIdAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserReadDto>> Create(UserCreateDto dto)
    {
        var created = await _users.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UserUpdateDto dto)
        => await _users.UpdateAsync(id, dto) ? NoContent() : NotFound();

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
        => await _users.DeleteAsync(id) ? NoContent() : NotFound();
}