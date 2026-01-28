namespace PawPairs.Domain.Entities;

public class Playdate
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PetAId { get; set; }
    public Pet PetA { get; set; } = null!;

    public Guid PetBId { get; set; }
    public Pet PetB { get; set; } = null!;

    public DateTime ScheduledAtUtc { get; set; }
    public string LocationName { get; set; } = null!;
    public string? Notes { get; set; }
}