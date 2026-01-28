using System;
using System.ComponentModel.DataAnnotations;

namespace PawPairs.Application.Dtos;

public record SchedulePlaydateDto(
    [Required] DateTime ScheduledAtUtc,
    [Required] [StringLength(200)] string LocationName,
    [StringLength(1000)] string? Notes
);