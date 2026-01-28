using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PawPairs.Application.Dtos;
using PawPairs.Application.Interfaces;
using PawPairs.Domain.Entities;
using PawPairs.Infrastructure.Data;

namespace PawPairs.Infrastructure.Services;

public class PlaydateService : IPlaydateService
{
    private readonly PawPairsDbContext _db;
    private readonly ILogger<PlaydateService> _logger;

    public PlaydateService(PawPairsDbContext db, ILogger<PlaydateService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<PlaydateReadDto>> GetAllAsync()
        => await _db.Playdates.AsNoTracking()
            .OrderByDescending(p => p.ScheduledAtUtc)
            .Select(p => new PlaydateReadDto(
                p.Id,
                p.PetAId,
                p.PetBId,
                p.ScheduledAtUtc,
                p.LocationName,
                p.Notes
            ))
            .ToListAsync();

    public async Task<List<PlaydateReadDto>> GetAllAsync(int skip, int take)
    {
        _logger.LogInformation("Retrieving playdates - Skip: {Skip}, Take: {Take}", skip, take);
        return await _db.Playdates.AsNoTracking()
            .OrderByDescending(p => p.ScheduledAtUtc)
            .Skip(skip)
            .Take(take)
            .Select(p => new PlaydateReadDto(
                p.Id,
                p.PetAId,
                p.PetBId,
                p.ScheduledAtUtc,
                p.LocationName,
                p.Notes
            ))
            .ToListAsync();
    }

    public async Task<int> GetCountAsync()
    {
        return await _db.Playdates.CountAsync();
    }

    public async Task<PlaydateReadDto?> GetByIdAsync(Guid id)
        => await _db.Playdates.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PlaydateReadDto(
                p.Id,
                p.PetAId,
                p.PetBId,
                p.ScheduledAtUtc,
                p.LocationName,
                p.Notes
            ))
            .FirstOrDefaultAsync();

    public async Task<PlaydateReadDto> CreateAsync(PlaydateCreateDto dto)
    {
        _logger.LogInformation("Creating playdate for PetA {PetAId} and PetB {PetBId}", dto.PetAId, dto.PetBId);
        
        if (dto.PetAId == dto.PetBId)
        {
            _logger.LogWarning("Attempted to create playdate with same pet IDs");
            throw new InvalidOperationException("PetA and PetB must be different.");
        }

        var petsExist = await _db.Pets.CountAsync(p => p.Id == dto.PetAId || p.Id == dto.PetBId);
        if (petsExist != 2)
        {
            _logger.LogWarning("One or both pets do not exist: PetAId={PetAId}, PetBId={PetBId}", dto.PetAId, dto.PetBId);
            throw new InvalidOperationException("One or both pets do not exist.");
        }

        var playdate = new Playdate
        {
            PetAId = dto.PetAId,
            PetBId = dto.PetBId,
            ScheduledAtUtc = dto.ScheduledAtUtc,
            LocationName = dto.LocationName,
            Notes = dto.Notes
        };

        _db.Playdates.Add(playdate);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created playdate {PlaydateId}", playdate.Id);

        return new PlaydateReadDto(
            playdate.Id,
            playdate.PetAId,
            playdate.PetBId,
            playdate.ScheduledAtUtc,
            playdate.LocationName,
            playdate.Notes
        );
    }

    public async Task<bool> UpdateAsync(Guid id, PlaydateUpdateDto dto)
    {
        var playdate = await _db.Playdates.FirstOrDefaultAsync(p => p.Id == id);
        if (playdate is null) return false;

        playdate.ScheduledAtUtc = dto.ScheduledAtUtc;
        playdate.LocationName = dto.LocationName;
        playdate.Notes = dto.Notes;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var playdate = await _db.Playdates.FirstOrDefaultAsync(p => p.Id == id);
        if (playdate is null) return false;

        _db.Playdates.Remove(playdate);
        await _db.SaveChangesAsync();
        return true;
    }
}
