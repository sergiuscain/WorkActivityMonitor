using Microsoft.AspNetCore.Mvc;
using WorkActivityMonitor.Dtos;
using WorkActivityMonitor.Services;

namespace WorkActivityMonitor.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientsController : ControllerBase
{
    private readonly ClientService _service;

    public ClientsController(ClientService service)
    {
        _service = service;
    }

    /// <summary>Проверка, что контроллер работает.</summary>
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new { status = "ok", timestamp = DateTime.UtcNow });
    }

    /// <summary>Список всех клиентов.</summary>
    [HttpGet]
    public async Task<ActionResult<List<ClientDto>>> GetAll(CancellationToken ct)
    {
        var clients = await _service.GetAllClientsAsync(ct);
        return Ok(clients);
    }
    /// <summary>Heartbeat от клиента. Сервер сам определяет IP из запроса.</summary>
    [HttpPost("heartbeat")]
    public async Task<ActionResult<HeartbeatResponse>> Heartbeat(
        [FromBody] HeartbeatRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.MachineName) ||
            string.IsNullOrWhiteSpace(request.UserName))
        {
            return BadRequest("MachineName and UserName are required.");
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var response = await _service.ProcessHeartbeatAsync(request, ip, ct);
        return Ok(response);
    }

    /// <summary>Запросить скриншот у клиента. Он сделает его при следующем heartbeat.</summary>
    [HttpPost("{id:int}/request-screenshot")]
    public async Task<IActionResult> RequestScreenshot(int id, CancellationToken ct)
    {
        var ok = await _service.RequestScreenshotAsync(id, ct);
        if (!ok) return NotFound();

        return Accepted(new
        {
            message = "Screenshot requested. Client will upload it on the next heartbeat.",
            clientId = id
        });
    }

    /// <summary>Клиент загружает скриншот (multipart/form-data).</summary>
    [HttpPost("{id:int}/screenshot")]
    [RequestSizeLimit(20 * 1024 * 1024)] // 20 MB
    public async Task<IActionResult> UploadScreenshot(int id, IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest("File is required.");

        await using var stream = file.OpenReadStream();
        var screenshot = await _service.SaveScreenshotAsync(id, stream, file.Length, ct);
        if (screenshot is null) return NotFound("Client not found.");

        return Ok(new
        {
            screenshot.Id,
            screenshot.ClientId,
            screenshot.FileSizeBytes,
            screenshot.CreatedAt
        });
    }

    /// <summary>Получить последний скриншот клиента (PNG).</summary>
    [HttpGet("{id:int}/screenshot")]
    public async Task<IActionResult> GetLatestScreenshot(int id, CancellationToken ct)
    {
        var screenshot = await _service.GetLatestScreenshotAsync(id, ct);
        if (screenshot is null) return NotFound("No screenshots yet.");

        if (!System.IO.File.Exists(screenshot.FilePath))
            return NotFound("Screenshot file missing on disk.");

        var bytes = await System.IO.File.ReadAllBytesAsync(screenshot.FilePath, ct);
        return File(bytes, "image/png", $"client_{id}_{screenshot.CreatedAt:yyyyMMdd_HHmmss}.png");
    }

    /// <summary>Список всех скриншотов клиента.</summary>
    [HttpGet("{id:int}/screenshots")]
    public async Task<IActionResult> GetAllScreenshots(int id, CancellationToken ct)
    {
        var list = await _service.GetAllScreenshotsAsync(id, ct);

        return Ok(list.Select(s => new
        {
            s.Id,
            s.ClientId,
            s.FileSizeBytes,
            s.CreatedAt
        }));
    }
}
