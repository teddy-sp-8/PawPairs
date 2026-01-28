using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PawPairs.Application.Dtos;
using PawPairs.Domain.Enums;

namespace PawPairs.Application.Interfaces;

public interface IPetService
{
    Task<List<PetReadDto>> GetAllAsync();
    Task<List<PetReadDto>> GetAllAsync(int skip, int take);
    Task<int> GetCountAsync();
    Task<PetReadDto?> GetByIdAsync(Guid id);
    Task<PetReadDto> CreateAsync(PetCreateDto dto);
    Task<bool> UpdateAsync(Guid id, PetUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);

    Task<List<PetReadDto>> SearchAsync(
        Species? species,
        EnergyLevel? energyLevel,
        double? nearLat,
        double? nearLng,
        double? radiusKm
    );
}