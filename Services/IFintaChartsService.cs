using Microsoft.AspNetCore.Mvc;
using Testing.Models;

namespace Testing.Services
{
    public interface IFintaChartsService
    {
        Task<IEnumerable<AssetPrice>> GetHistoricalPriceAsync(string symbol);

        Task<IEnumerable<string>> GetSupportedAssetsAsync();

        Task<string> GetTokenAsync();
    }
}
