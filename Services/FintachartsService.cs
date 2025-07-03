using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Metrics;
using System.Net.Http.Headers;
using System.Text.Json;
using Testing.Models;

namespace Testing.Services
{
    public class FintachartsService : IFintaChartsService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public FintachartsService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }
        public async Task<IEnumerable<AssetPrice>> GetHistoricalPriceAsync(string symbol)
        {
            string token = await GetTokenAsync();

            // 1. Отримуємо всі інструменти
            var instrumentsRequest = new HttpRequestMessage(HttpMethod.Get,
                $"{_config["Fintacharts:Uri"]}/api/instruments/v1/instruments?provider=oanda&kind=forex");

            instrumentsRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var instrumentsResponse = await _httpClient.SendAsync(instrumentsRequest);
            instrumentsResponse.EnsureSuccessStatusCode();

            var instrumentsJson = await instrumentsResponse.Content.ReadAsStringAsync();
            var instruments = JsonSerializer.Deserialize<JsonElement>(instrumentsJson);

            // 2. Знаходимо інструмент за символом
            var instrument = instruments.GetProperty("items").EnumerateArray()
                .FirstOrDefault(x => x.GetProperty("symbol").GetString() == symbol);

            if (instrument.ValueKind == JsonValueKind.Undefined)
                throw new Exception($"Instrument with symbol {symbol} not found.");

            string instrumentId = instrument.GetProperty("id").GetString()!;

            // 3. Отримуємо історичні дані по цьому instrumentId
            var barsUrl = $"{_config["Fintacharts:Uri"]}/api/bars/v1/bars/count-back" +
                $"?instrumentId={instrumentId}&provider=oanda&interval=1&periodicity=minute&barsCount=10";

            var barsRequest = new HttpRequestMessage(HttpMethod.Get, barsUrl);
            barsRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var barsResponse = await _httpClient.SendAsync(barsRequest);
            barsResponse.EnsureSuccessStatusCode();

            var barsJson = await barsResponse.Content.ReadAsStringAsync();

            // 4. Десеріалізуємо у список цін
            var barsData = JsonSerializer.Deserialize<JsonElement>(barsJson);
            var items = barsData.GetProperty("items");

            var prices = new List<AssetPrice>();
            foreach (var item in items.EnumerateArray())
            {
                prices.Add(new AssetPrice
                {
                    Symbol = symbol,
                    Price = item.GetProperty("close").GetDecimal(),
                    Update = item.GetProperty("time").GetDateTime()
                });
            }

            return prices;
        }


        public async Task<string> GetTokenAsync()
        {
            var url = $"{_config["Fintacharts:Uri"]}/identity/realms/fintatech/protocol/openid-connect/token";

            var content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("client_id", "app-cli"),
            new KeyValuePair<string, string>("username", _config["Fintacharts:Username"]),
            new KeyValuePair<string, string>("password", _config["Fintacharts:Password"])
            });

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)  
                throw new Exception("Auth failed");

            var json = await response.Content.ReadAsStringAsync();

            var data = JsonSerializer.Deserialize<JsonElement>(json);

            var token = data.GetProperty("access_token").GetString();

            return token!;
        }


        public async Task<IEnumerable<string>> GetSupportedAssetsAsync()
        {
            string token = await GetTokenAsync();

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_config["Fintacharts:Uri"]}/api/instruments/v1/instruments?provider=oanda&kind=forex");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var responce = await _httpClient.SendAsync(request);
            responce.EnsureSuccessStatusCode();

            var json = await responce.Content.ReadAsStringAsync();
            var instruments = JsonSerializer.Deserialize<JsonElement>(json);

            return instruments.GetProperty("items")
                .EnumerateArray()
                .Select(x => x.GetProperty("symbol").GetString()!)
                .ToList();
        }
    }
}
