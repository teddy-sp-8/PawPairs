using System.Threading;
using System.Threading.Tasks;
using PawPairs.Application.Dtos;

namespace PawPairs.Application.Interfaces;

public interface ICatApiClient
{
    Task<CatBreedsApiResponse> GetBreedsPageAsync(int page, CancellationToken ct = default);
}