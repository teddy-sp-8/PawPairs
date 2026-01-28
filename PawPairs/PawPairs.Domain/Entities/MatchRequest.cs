using PawPairs.Domain.Enums;

namespace PawPairs.Domain.Entities;

public class MatchRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid FromPetId { get; set; }
    public Pet FromPet { get; set; } = null!;

    public Guid ToPetId { get; set; }
    public Pet ToPet { get; set; } = null!;

    public MatchStatus Status { get; set; } = MatchStatus.Pending;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}