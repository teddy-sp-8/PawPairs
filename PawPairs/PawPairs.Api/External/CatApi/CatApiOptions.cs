namespace PawPairs.Api.External.CatApi;

public class CatApiOptions
{
    public string BaseUrl { get; set; } = "https://catfact.ninja/";
    public int TimeoutSeconds { get; set; } = 10;
}