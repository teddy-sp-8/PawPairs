namespace PawPairs.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;
    public string City { get; set; } = null!;

    // Navigation
    public List<Pet> Pets { get; set; } = [];
}