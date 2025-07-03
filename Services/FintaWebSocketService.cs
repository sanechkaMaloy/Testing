using System.Net.WebSockets;
using System.Text.Json;
using System.Text;

namespace Testing.Services
{
    public class FintaWebSocketService: BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<FintaWebSocketService> _logger;
        private readonly IFintaChartsService _fintachartsService;

        public FintaWebSocketService(IConfiguration configuration, ILogger<FintaWebSocketService> logger, IFintaChartsService fintachartsService)
        {
            _config = configuration;
            _logger = logger;
            _fintachartsService = fintachartsService;
        }

            protected override async Task ExecuteAsync(CancellationToken stoppingToken)
            {
                var Url = _config["Fintacharts:WebSocketUri"];
                var token = await _fintachartsService.GetTokenAsync();


            using var client = new ClientWebSocket();
                client.Options.SetRequestHeader("Authorization", $"Bearer {token}");

                await client.ConnectAsync(new Uri(Url!), stoppingToken);

                var subscribeMessage = new
                {
                    op = "subscribe",
                    args = new[]
                    {
                        new
                        {
                            channel = "tickers",
                            symbol = "EUR/USD"
                        }
                    }
                };

                string json = JsonSerializer.Serialize(subscribeMessage);
                var bytes = Encoding.UTF8.GetBytes(json);

                await client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, stoppingToken);

                var buffer = new byte[4096];

                while (!stoppingToken.IsCancellationRequested)
                {
                    var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogWarning("WebSocket closed");
                        break;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    _logger.LogInformation($"Realtime: {message}");
                }
            }
        }
}
