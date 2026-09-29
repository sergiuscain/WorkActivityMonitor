using Client.Core;
using Client.Models;

namespace Client
{
    internal class TrayApplicationContext : ApplicationContext
    {
        private readonly System.Windows.Forms.Timer _timer;
        private readonly ApiClient _api;

        // Позже вынесу в appsettings.json
        private const string ServerUrl = "https://localhost:7068";

        public TrayApplicationContext()
        {
            SimpleLogger.Log("Application started");

            _api = new ApiClient(ServerUrl);

            _timer = new System.Windows.Forms.Timer { Interval = 10_000 };
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        private async void OnTimerTick(object? sender, EventArgs e)
        {
            try
            {
                var request = new HeartbeatRequest
                {
                    MachineName = Environment.MachineName,
                    UserName = Environment.UserName,
                    Domain = Environment.UserDomainName
                };

                var response = await _api.SendHeartbeatAsync(request);

                if (response != null)
                {
                    SimpleLogger.Log($"Heartbeat OK: ClientId={response.ClientId}, TakeScreenshot={response.TakeScreenshot}");
                }
            }
            catch (Exception ex)
            {
                // Последняя защита: async void не должен уронить приложение
                SimpleLogger.Log($"Tick error: {ex.Message}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer?.Stop();
                _timer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}