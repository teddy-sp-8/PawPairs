using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PawPairs.Application.Dtos;

namespace PawPairs.Application.Interfaces;

public interface IPlaydateService
{
    Task<List<PlaydateReadDto>> GetAllAsync();
    Task<List<PlaydateReadDto>> GetAllAsync(int skip, int take);
    Task<int> GetCountAsync();
    Task<PlaydateReadDto?> GetByIdAsync(Guid id);
    Task<PlaydateReadDto> CreateAsync(PlaydateCreateDto dto);
    Task<bool> UpdateAsync(Guid id, PlaydateUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);
}