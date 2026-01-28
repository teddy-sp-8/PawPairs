using System;
using System.ComponentModel.DataAnnotations;

namespace PawPairs.Application.Dtos;

public record UserCreateDto(
    [Required] [StringLength(100)] string FirstName,
    [Required] [StringLength(100)] string LastName,
    [Required] [EmailAddress] [StringLength(255)] string Email,
    [Required] [StringLength(100)] string City
);

public record UserUpdateDto(
    [Required] [StringLength(100)] string FirstName,
    [Required] [StringLength(100)] string LastName,
    [Required] [StringLength(100)] string City
);

public record UserReadDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string City
);