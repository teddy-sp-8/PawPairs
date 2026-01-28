using System.Collections.Generic;

namespace PawPairs.Application.Dtos;

public record CatBreedDto(
    string Breed,
    string Country,
    string Origin,
    string Coat,
    string Pattern,
    string CoatCategory
);

public record CatBreedsApiResponse(
    int Current_Page,
    List<CatBreedApiItem> Data,
    int Last_Page,
    int Per_Page,
    int Total,
    string? Next_Page_Url,
    string? Prev_Page_Url
);

public record CatBreedApiItem(
    string? Breed,
    string? Country,
    string? Origin,
    string? Coat,
    string? Pattern
);
