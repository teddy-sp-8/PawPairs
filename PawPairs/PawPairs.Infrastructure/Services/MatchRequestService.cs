using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PawPairs.Application.Dtos;
using PawPairs.Application.Interfaces;
using PawPairs.Domain.Entities;
using PawPairs.Domain.Enums;
using PawPairs.Infrastructure.Data;

namespace PawPairs.Infrastructure.Services;

public class MatchRequestService : IMatchRequestService
{
    private readonly PawPairsDbContext _db;
    private readonly ILogger<MatchRequestService> _logger;

    public MatchRequestService(PawPairsDbContext db, ILogger<MatchRequestService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<MatchRequestReadDto>> GetAllAsync()
        => await _db.MatchRequests.AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new MatchRequestReadDto(x.Id, x.FromPetId, x.ToPetId, x.Status, x.CreatedAtUtc))
            .ToListAsync();

    public async Task<List<MatchRequestReadDto>> GetAllAsync(int skip, int take)
    {
        _logger.LogInformation("Retrieving match requests - Skip: {Skip}, Take: {Take}", skip, take);
        return await _db.MatchRequests.AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip(skip)
            .Take(take)
            .Select(x => new MatchRequestReadDto(x.Id, x.FromPetId, x.ToPetId, x.Status, x.CreatedAtUtc))
            .ToListAsync();
    }

    public async Task<int> GetCountAsync()
    {
        return await _db.MatchRequests.CountAsync();
    }

    public async Task<MatchRequestReadDto?> GetByIdAsync(Guid id)
        => await _db.MatchRequests.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new MatchRequestReadDto(x.Id, x.FromPetId, x.ToPetId, x.Status, x.CreatedAtUtc))
            .FirstOrDefaultAsync();

    public async Task<MatchRequestReadDto> CreateAsync(MatchRequestCreateDto dto)
    {
        _logger.LogInformation("Creating match request from Pet {FromPetId} to Pet {ToPetId}", dto.FromPetId, dto.ToPetId);
        
        if (dto.FromPetId == dto.ToPetId)
        {
            _logger.LogWarning("Attempted to create match request with same pet IDs");
            throw new InvalidOperationException("FromPet and ToPet must be different.");
        }

        var petsExist = await _db.Pets.CountAsync(p => p.Id == dto.FromPetId || p.Id == dto.ToPetId);
        if (petsExist != 2)
        {
            _logger.LogWarning("One or both pets do not exist: FromPetId={FromPetId}, ToPetId={ToPetId}", dto.FromPetId, dto.ToPetId);
            throw new InvalidOperationException("One or both pets do not exist.");
        }

        var alreadyPending = await _db.MatchRequests.AnyAsync(x =>
            x.FromPetId == dto.FromPetId &&
            x.ToPetId == dto.ToPetId &&
            x.Status == MatchStatus.Pending);

        if (alreadyPending)
        {
            _logger.LogWarning("Pending match request already exists from Pet {FromPetId} to Pet {ToPetId}", dto.FromPetId, dto.ToPetId);
            throw new InvalidOperationException("A pending match request already exists.");
        }

        var req = new MatchRequest
        {
            FromPetId = dto.FromPetId,
            ToPetId = dto.ToPetId,
            Status = MatchStatus.Pending
        };

        _db.MatchRequests.Add(req);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created match request {RequestId}", req.Id);

        return new MatchRequestReadDto(req.Id, req.FromPetId, req.ToPetId, req.Status, req.CreatedAtUtc);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var req = await _db.MatchRequests.FirstOrDefaultAsync(x => x.Id == id);
        if (req is null) return false;

        _db.MatchRequests.Remove(req);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AcceptAsync(Guid id)
    {
        _logger.LogInformation("Accepting match request {RequestId}", id);
        var req = await _db.MatchRequests.FirstOrDefaultAsync(x => x.Id == id);
        if (req is null)
        {
            _logger.LogWarning("Match request {RequestId} not found for acceptance", id);
            return false;
        }
        if (req.Status != MatchStatus.Pending)
        {
            _logger.LogWarning("Match request {RequestId} is not pending (status: {Status})", id, req.Status);
            return false;
        }

        req.Status = MatchStatus.Accepted;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Accepted match request {RequestId}", id);
        return true;
    }

    public async Task<bool> RejectAsync(Guid id)
    {
        _logger.LogInformation("Rejecting match request {RequestId}", id);
        var req = await _db.MatchRequests.FirstOrDefaultAsync(x => x.Id == id);
        if (req is null)
        {
            _logger.LogWarning("Match request {RequestId} not found for rejection", id);
            return false;
        }
        if (req.Status != MatchStatus.Pending)
        {
            _logger.LogWarning("Match request {RequestId} is not pending (status: {Status})", id, req.Status);
            return false;
        }

        req.Status = MatchStatus.Rejected;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Rejected match request {RequestId}", id);
        return true;
    }
    
    public async Task<List<MatchRequestReadDto>> GetIncomingAsync(Guid toPetId)
        => await _db.MatchRequests.AsNoTracking()
            .Where(x => x.ToPetId == toPetId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new MatchRequestReadDto(
                x.Id, x.FromPetId, x.ToPetId, x.Status, x.CreatedAtUtc))
            .ToListAsync();

    public async Task<List<MatchRequestReadDto>> GetOutgoingAsync(Guid fromPetId)
        => await _db.MatchRequests.AsNoTracking()
            .Where(x => x.FromPetId == fromPetId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new MatchRequestReadDto(
                x.Id, x.FromPetId, x.ToPetId, x.Status, x.CreatedAtUtc))
            .ToListAsync();
    
    public async Task<List<MatchRequestReadDto>> GetPendingIncomingAsync(Guid toPetId)
        => await _db.MatchRequests.AsNoTracking()
            .Where(x => x.ToPetId == toPetId && x.Status == MatchStatus.Pending)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new MatchRequestReadDto(
                x.Id, x.FromPetId, x.ToPetId, x.Status, x.CreatedAtUtc))
            .ToListAsync();

    public async Task<List<MatchRequestReadDto>> GetPendingOutgoingAsync(Guid fromPetId)
        => await _db.MatchRequests.AsNoTracking()
            .Where(x => x.FromPetId == fromPetId && x.Status == MatchStatus.Pending)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new MatchRequestReadDto(
                x.Id, x.FromPetId, x.ToPetId, x.Status, x.CreatedAtUtc))
            .ToListAsync();
    public async Task<PlaydateReadDto> SchedulePlaydateAsync(Guid matchRequestId, SchedulePlaydateDto dto)
    {
        _logger.LogInformation("Scheduling playdate for match request {RequestId}", matchRequestId);
        var req = await _db.MatchRequests.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == matchRequestId);

        if (req is null)
        {
            _logger.LogWarning("Match request {RequestId} not found for scheduling playdate", matchRequestId);
            throw new InvalidOperationException("Match request not found.");
        }

        if (req.Status != MatchStatus.Accepted)
        {
            _logger.LogWarning("Match request {RequestId} is not accepted (status: {Status})", matchRequestId, req.Status);
            throw new InvalidOperationException("Match request must be Accepted before scheduling a playdate.");
        }

        var petsExist = await _db.Pets.CountAsync(p => p.Id == req.FromPetId || p.Id == req.ToPetId);
        if (petsExist != 2)
        {
            _logger.LogWarning("One or both pets no longer exist for match request {RequestId}", matchRequestId);
            throw new InvalidOperationException("One or both pets no longer exist.");
        }

        var playdate = new Playdate
        {
            PetAId = req.FromPetId,
            PetBId = req.ToPetId,
            ScheduledAtUtc = dto.ScheduledAtUtc,
            LocationName = dto.LocationName,
            Notes = dto.Notes
        };

        _db.Playdates.Add(playdate);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Scheduled playdate {PlaydateId} for match request {RequestId}", playdate.Id, matchRequestId);

        return new PlaydateReadDto(
            playdate.Id,
            playdate.PetAId,
            playdate.PetBId,
            playdate.ScheduledAtUtc,
            playdate.LocationName,
            playdate.Notes
        );
    }

}
