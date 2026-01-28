using System;
using System.ComponentModel.DataAnnotations;
using PawPairs.Domain.Enums;

namespace PawPairs.Application.Dtos;

public record MatchRequestCreateDto(
    [Required] Guid FromPetId,
    [Required] Guid ToPetId
);

public record MatchRequestReadDto(
    Guid Id,
    Guid FromPetId,
    Guid ToPetId,
    MatchStatus Status,
    DateTime CreatedAtUtc
);