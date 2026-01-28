using PawPairs.Domain.Enums;

namespace PawPairs.Domain.Entities;

public class Pet
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public string Name { get; set; } = null!;
    public Species Species { get; set; }
    public string BreedName { get; set; } = null!;

    public int AgeYears { get; set; }
    public EnergyLevel EnergyLevel { get; set; }
    public bool IsVaccinated { get; set; }

    // “Nearby pets” support (simple)
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}