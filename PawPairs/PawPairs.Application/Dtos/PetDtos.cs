using System;
using System.ComponentModel.DataAnnotations;
using PawPairs.Domain.Enums;

namespace PawPairs.Application.Dtos;

public record PetCreateDto(
    [Required] Guid OwnerId,
    [Required] [StringLength(100)] string Name,
    [Required] Species Species,
    [Required] [StringLength(100)] string BreedName,
    [Range(0, 30)] int AgeYears,
    [Required] EnergyLevel EnergyLevel,
    bool IsVaccinated,
    [Range(-90, 90)] double Latitude,
    [Range(-180, 180)] double Longitude
);

public record PetUpdateDto(
    [Required] [StringLength(100)] string Name,
    [Required] [StringLength(100)] string BreedName,
    [Range(0, 30)] int AgeYears,
    [Required] EnergyLevel EnergyLevel,
    bool IsVaccinated,
    [Range(-90, 90)] double Latitude,
    [Range(-180, 180)] double Longitude
);

public record PetReadDto(
    Guid Id,
    Guid OwnerId,
    string Name,
    Species Species,
    string BreedName,
    int AgeYears,
    EnergyLevel EnergyLevel,
    bool IsVaccinated,
    double Latitude,
    double Longitude
);