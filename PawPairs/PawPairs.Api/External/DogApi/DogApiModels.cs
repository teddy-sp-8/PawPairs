using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PawPairs.Api.External.DogApi;

public sealed class DogApiBreedsResponse
{
    [JsonPropertyName("data")]
    public List<DogApiBreedItem> Data { get; set; } = [];
}

public sealed class DogApiBreedItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("attributes")]
    public DogApiBreedAttributes Attributes { get; set; } = new();
}

public sealed class DogApiBreedAttributes
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("hypoallergenic")]
    public bool? Hypoallergenic { get; set; }

    [JsonPropertyName("life")]
    public DogApiMinMax? Life { get; set; }

    [JsonPropertyName("male_weight")]
    public DogApiMinMax? MaleWeight { get; set; }

    [JsonPropertyName("female_weight")]
    public DogApiMinMax? FemaleWeight { get; set; }
}

public sealed class DogApiMinMax
{
    [JsonPropertyName("min")]
    public int? Min { get; set; }

    [JsonPropertyName("max")]
    public int? Max { get; set; }
}