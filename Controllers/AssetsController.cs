using Microsoft.AspNetCore.Mvc;
using Testing.Data;
using Testing.Models;
using Testing.Services;

namespace Testing.Controllers
{
    [ApiController]
    [Route("api/assets")]
    public class AssetsController : ControllerBase
    {
        private readonly IFintaChartsService _service;
        private readonly AppDbContext _context;

        public AssetsController(IFintaChartsService service, AppDbContext context)
        {
            _service = service;
            _context = context;
        }

        [HttpPost("{symbol}/history/save")]
        public async Task<IActionResult> SaveHistory(string symbol)
        {
            var prices = await _service.GetHistoricalPriceAsync(symbol);

            if (prices == null)
            {
                return NotFound("No price history");
            }

            await _context.AssetPrices.AddRangeAsync(prices);
            await _context.SaveChangesAsync();

            return Ok($"Records saved");
        }

        // GET /api/assets
        [HttpGet]
        public async Task<IActionResult> GetAssets()
        {
            var assets = await _service.GetSupportedAssetsAsync();
            return Ok(assets);
        }

        // GET /api/assets/{symbol}/history
        [HttpGet("{symbol}/history")]
        public async Task<IActionResult> GetHistory(string symbol)
        {
            var history = await _service.GetHistoricalPriceAsync(symbol);
            return Ok(history);
        }
    }

}
