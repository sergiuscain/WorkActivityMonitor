using Client.Models;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Client.Core
{
    internal class ApiClient
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _http;
        private readonly string _baseUrl;

        public ApiClient(string baseUrl)
        {
            _baseUrl = baseUrl.TrimEnd('/');

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            _http = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30),
                DefaultRequestVersion = HttpVersion.Version11,
                DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact
            };
        }

        public async Task<HeartbeatResponse?> SendHeartbeatAsync(HeartbeatRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request, JsonOptions);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"{_baseUrl}/api/clients/heartbeat";
                using var response = await _http.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    SimpleLogger.Log($"Heartbeat failed: {(int)response.StatusCode} {response.ReasonPhrase}");
                    return null;
                }

                var body = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<HeartbeatResponse>(body, JsonOptions);
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"Heartbeat exception: {ex.Message}");
                SimpleLogger.Log($"Inner: {ex.InnerException?.Message}");
                return null;
            }
        }

        public async Task<bool> UploadScreenshotAsync(int clientId, byte[] pngBytes)
        {
            try
            {
                var url = $"{_baseUrl}/api/clients/{clientId}/screenshot";

                using var form = new MultipartFormDataContent();
                using var fileContent = new ByteArrayContent(pngBytes);
                fileContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");

                form.Add(fileContent, "file", $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png");

                using var response = await _http.PostAsync(url, form);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    SimpleLogger.Log($"Screenshot upload failed: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {body}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"Screenshot upload exception: {ex.Message}");
                SimpleLogger.Log($"Inner: {ex.InnerException?.Message}");
                return false;
            }
        }
    }
}