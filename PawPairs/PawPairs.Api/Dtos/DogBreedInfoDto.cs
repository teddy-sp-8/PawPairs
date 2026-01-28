namespace PawPairs.Api.Dtos;

public record DogBreedInfoDto(
    string Name,
    string? Description,
    bool Hypoallergenic,
    int? LifeMinYears,
    int? LifeMaxYears,
    int? MaleWeightMinKg,
    int? MaleWeightMaxKg,
    int? FemaleWeightMinKg,
    int? FemaleWeightMaxKg,
    string SizeCategory
);