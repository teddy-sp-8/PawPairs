using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PawPairs.Application.Dtos;

namespace PawPairs.Application.Interfaces;

public interface IUserService
{
    Task<List<UserReadDto>> GetAllAsync();
    Task<List<UserReadDto>> GetAllAsync(int skip, int take);
    Task<int> GetCountAsync();
    Task<UserReadDto?> GetByIdAsync(Guid id);
    Task<UserReadDto> CreateAsync(UserCreateDto dto);
    Task<bool> UpdateAsync(Guid id, UserUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);
}