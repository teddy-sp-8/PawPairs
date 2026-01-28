using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PawPairs.Application.Dtos;
using PawPairs.Application.Interfaces;
using PawPairs.Domain.Entities;
using PawPairs.Domain.Enums;
using PawPairs.Infrastructure.Data;

namespace PawPairs.Infrastructure.Services;

public class PetService : IPetService
{
    private readonly PawPairsDbContext _db;
    private readonly ILogger<PetService> _logger;

    public PetService(PawPairsDbContext db, ILogger<PetService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<PetReadDto>> GetAllAsync() =>
        await _db.Pets.AsNoTracking()
            .Select(p => new PetReadDto(p.Id, p.OwnerId, p.Name, p.Species, p.BreedName, p.AgeYears, p.EnergyLevel,
                p.IsVaccinated, p.Latitude, p.Longitude))
            .ToListAsync();

    public async Task<List<PetReadDto>> GetAllAsync(int skip, int take)
    {
        _logger.LogInformation("Retrieving pets - Skip: {Skip}, Take: {Take}", skip, take);
        return await _db.Pets.AsNoTracking()
            .OrderBy(p => p.Name)
            .Skip(skip)
            .Take(take)
            .Select(p => new PetReadDto(p.Id, p.OwnerId, p.Name, p.Species, p.BreedName, p.AgeYears, p.EnergyLevel,
                p.IsVaccinated, p.Latitude, p.Longitude))
            .ToListAsync();
    }

    public async Task<int> GetCountAsync()
    {
        return await _db.Pets.CountAsync();
    }

    public async Task<PetReadDto?> GetByIdAsync(Guid id) =>
        await _db.Pets.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PetReadDto(p.Id, p.OwnerId, p.Name, p.Species, p.BreedName, p.AgeYears, p.EnergyLevel,
                p.IsVaccinated, p.Latitude, p.Longitude))
            .FirstOrDefaultAsync();

    public async Task<PetReadDto> CreateAsync(PetCreateDto dto)
    {
        _logger.LogInformation("Creating pet {PetName} for owner {OwnerId}", dto.Name, dto.OwnerId);
        var ownerExists = await _db.Users.AnyAsync(u => u.Id == dto.OwnerId);
        if (!ownerExists)
        {
            _logger.LogWarning("Owner {OwnerId} not found when creating pet", dto.OwnerId);
            throw new InvalidOperationException("Owner (User) not found.");
        }

        var pet = new Pet
        {
            OwnerId = dto.OwnerId,
            Name = dto.Name,
            Species = dto.Species,
            BreedName = dto.BreedName,
            AgeYears = dto.AgeYears,
            EnergyLevel = dto.EnergyLevel,
            IsVaccinated = dto.IsVaccinated,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude
        };

        _db.Pets.Add(pet);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created pet {PetId} ({PetName})", pet.Id, pet.Name);

        return new PetReadDto(pet.Id, pet.OwnerId, pet.Name, pet.Species, pet.BreedName, pet.AgeYears, pet.EnergyLevel,
            pet.IsVaccinated, pet.Latitude, pet.Longitude);
    }

    public async Task<bool> UpdateAsync(Guid id, PetUpdateDto dto)
    {
        var pet = await _db.Pets.FirstOrDefaultAsync(p => p.Id == id);
        if (pet is null) return false;

        pet.Name = dto.Name;
        pet.BreedName = dto.BreedName;
        pet.AgeYears = dto.AgeYears;
        pet.EnergyLevel = dto.EnergyLevel;
        pet.IsVaccinated = dto.IsVaccinated;
        pet.Latitude = dto.Latitude;
        pet.Longitude = dto.Longitude;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var pet = await _db.Pets.FirstOrDefaultAsync(p => p.Id == id);
        if (pet is null) return false;

        _db.Pets.Remove(pet);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<PetReadDto>> SearchAsync(Species? species, EnergyLevel? energyLevel, double? nearLat,
        double? nearLng, double? radiusKm)
    {
        _logger.LogInformation("Searching pets - Species: {Species}, EnergyLevel: {EnergyLevel}, Location: ({Lat}, {Lng}), Radius: {Radius}km",
            species, energyLevel, nearLat, nearLng, radiusKm);
        
        var q = _db.Pets.AsNoTracking().AsQueryable();

        if (species.HasValue) q = q.Where(p => p.Species == species.Value);

        if (energyLevel.HasValue) q = q.Where(p => p.EnergyLevel == energyLevel.Value);

        if (!nearLat.HasValue || !nearLng.HasValue || radiusKm is not > 0)
        {
            var results = await q.Select(p => new PetReadDto(p.Id, p.OwnerId, p.Name, p.Species, p.BreedName, p.AgeYears,
                    p.EnergyLevel, p.IsVaccinated, p.Latitude, p.Longitude))
                .ToListAsync();
            _logger.LogInformation("Search returned {Count} pets (no location filter)", results.Count);
            return results;
        }

        // Haversine formula for accurate distance calculation
        const double earthRadiusKm = 6371.0;
        var lat1Rad = nearLat.Value * Math.PI / 180.0;
        var latDelta = radiusKm.Value / earthRadiusKm;
        var lngDelta = radiusKm.Value / (earthRadiusKm * Math.Cos(lat1Rad));

        var minLat = nearLat.Value - (latDelta * 180.0 / Math.PI);
        var maxLat = nearLat.Value + (latDelta * 180.0 / Math.PI);
        var minLng = nearLng.Value - (lngDelta * 180.0 / Math.PI);
        var maxLng = nearLng.Value + (lngDelta * 180.0 / Math.PI);

        q = q.Where(p =>
            p.Latitude >= minLat && p.Latitude <= maxLat && p.Longitude >= minLng && p.Longitude <= maxLng);

        var resultsWithLocation = await q.Select(p => new PetReadDto(p.Id, p.OwnerId, p.Name, p.Species, p.BreedName, p.AgeYears,
                p.EnergyLevel, p.IsVaccinated, p.Latitude, p.Longitude))
            .ToListAsync();
        
        _logger.LogInformation("Search returned {Count} pets (with location filter)", resultsWithLocation.Count);
        return resultsWithLocation;
    }
}