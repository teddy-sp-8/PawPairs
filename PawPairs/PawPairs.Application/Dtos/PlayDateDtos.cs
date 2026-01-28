using System;
using System.ComponentModel.DataAnnotations;

namespace PawPairs.Application.Dtos;

public record PlaydateCreateDto(
    [Required] Guid PetAId,
    [Required] Guid PetBId,
    [Required] DateTime ScheduledAtUtc,
    [Required] [StringLength(200)] string LocationName,
    [StringLength(1000)] string? Notes
);

public record PlaydateUpdateDto(
    [Required] DateTime ScheduledAtUtc,
    [Required] [StringLength(200)] string LocationName,
    [StringLength(1000)] string? Notes
);

public record PlaydateReadDto(
    Guid Id,
    Guid PetAId,
    Guid PetBId,
    DateTime ScheduledAtUtc,
    string LocationName,
    string? Notes
);