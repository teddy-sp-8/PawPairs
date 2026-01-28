using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PawPairs.Application.Dtos;

namespace PawPairs.Application.Interfaces;

public interface IMatchRequestService
{
    Task<List<MatchRequestReadDto>> GetAllAsync();
    Task<List<MatchRequestReadDto>> GetAllAsync(int skip, int take);
    Task<int> GetCountAsync();
    Task<MatchRequestReadDto?> GetByIdAsync(Guid id);
    Task<MatchRequestReadDto> CreateAsync(MatchRequestCreateDto dto);
    Task<bool> DeleteAsync(Guid id);

    // Extra action ✅
    Task<bool> AcceptAsync(Guid id);
    Task<bool> RejectAsync(Guid id);
    Task<List<MatchRequestReadDto>> GetIncomingAsync(Guid toPetId);
    Task<List<MatchRequestReadDto>> GetOutgoingAsync(Guid fromPetId);
    
    Task<List<MatchRequestReadDto>> GetPendingIncomingAsync(Guid toPetId);
    Task<List<MatchRequestReadDto>> GetPendingOutgoingAsync(Guid fromPetId);
    Task<PlaydateReadDto> SchedulePlaydateAsync(Guid matchRequestId, SchedulePlaydateDto dto);


}