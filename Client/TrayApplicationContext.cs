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

                if (response == null)
                {
                    return;
                }

                SimpleLogger.Log($"Heartbeat OK: ClientId={response.ClientId}, TakeScreenshot={response.TakeScreenshot}");

                if (response.TakeScreenshot)
                {
                    try
                    {
                        var png = ScreenshotService.CaptureScreen();
                        SimpleLogger.Log($"Screenshot captured: {png.Length} bytes");

                        var uploaded = await _api.UploadScreenshotAsync(response.ClientId, png);

                        if (uploaded)
                        {
                            SimpleLogger.Log("Screenshot uploaded successfully");
                        }
                        else
                        {
                            SimpleLogger.Log("Screenshot upload failed");
                        }
                    }
                    catch (Exception ex)
                    {
                        SimpleLogger.Log($"Screenshot processing failed: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
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