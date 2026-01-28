using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PawPairs.Application.Dtos;
using PawPairs.Application.Interfaces;
using PawPairs.Domain.Entities;
using PawPairs.Infrastructure.Data;

namespace PawPairs.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly PawPairsDbContext _db;
    private readonly ILogger<UserService> _logger;

    public UserService(PawPairsDbContext db, ILogger<UserService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<UserReadDto>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all users");
        var users = await _db.Users
            .AsNoTracking()
            .Select(u => new UserReadDto(u.Id, u.FirstName, u.LastName, u.Email, u.City))
            .ToListAsync();
        _logger.LogInformation("Retrieved {Count} users", users.Count);
        return users;
    }

    public async Task<List<UserReadDto>> GetAllAsync(int skip, int take)
    {
        _logger.LogInformation("Retrieving users - Skip: {Skip}, Take: {Take}", skip, take);
        var users = await _db.Users
            .AsNoTracking()
            .OrderBy(u => u.Email)
            .Skip(skip)
            .Take(take)
            .Select(u => new UserReadDto(u.Id, u.FirstName, u.LastName, u.Email, u.City))
            .ToListAsync();
        _logger.LogInformation("Retrieved {Count} users", users.Count);
        return users;
    }

    public async Task<int> GetCountAsync()
    {
        return await _db.Users.CountAsync();
    }

    public async Task<UserReadDto?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Retrieving user with ID {UserId}", id);
        var user = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserReadDto(u.Id, u.FirstName, u.LastName, u.Email, u.City))
            .FirstOrDefaultAsync();
        
        if (user is null)
            _logger.LogWarning("User with ID {UserId} not found", id);
        
        return user;
    }

    public async Task<UserReadDto> CreateAsync(UserCreateDto dto)
    {
        _logger.LogInformation("Creating new user with email {Email}", dto.Email);
        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            City = dto.City
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created user with ID {UserId}", user.Id);

        return new UserReadDto(user.Id, user.FirstName, user.LastName, user.Email, user.City);
    }

    public async Task<bool> UpdateAsync(Guid id, UserUpdateDto dto)
    {
        _logger.LogInformation("Updating user with ID {UserId}", id);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            _logger.LogWarning("User with ID {UserId} not found for update", id);
            return false;
        }

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.City = dto.City;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Updated user with ID {UserId}", id);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting user with ID {UserId}", id);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            _logger.LogWarning("User with ID {UserId} not found for deletion", id);
            return false;
        }

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Deleted user with ID {UserId}", id);
        return true;
    }
}