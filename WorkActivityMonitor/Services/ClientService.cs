using Microsoft.EntityFrameworkCore;
using WorkActivityMonitor.Data;
using WorkActivityMonitor.Dtos;
using WorkActivityMonitor.Models;

namespace WorkActivityMonitor.Services;

public class ClientService
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ClientService> _logger;

    // Максимально допустимое время для того, что бы клиент считался онлайн
    private const int OnlineThresholdSeconds = 30;
    public ClientService(AppDbContext db, IWebHostEnvironment env, ILogger<ClientService> logger)
    {
        _db = db;
        _env = env;
        _logger = logger;
    }

    public async Task<List<ClientDto>> GetAllClientsAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var clients = await _db.Clients
            .Include(c => c.Screenshots)
            .OrderByDescending(c => c.LastActiveTime)
            .ToListAsync(ct);

        return clients.Select(c => new ClientDto
        {
            Id = c.Id,
            MachineName = c.MachineName,
            UserName = c.UserName,
            Domain = c.Domain,
            IpAddress = c.IpAddress,
            LastActiveTime = c.LastActiveTime,
            IsOnline = (now - c.LastActiveTime).TotalSeconds < OnlineThresholdSeconds,
            ScreenshotCount = c.Screenshots.Count
        }).ToList();
    }

    public async Task<HeartbeatResponse> ProcessHeartbeatAsync(
    HeartbeatRequest request,
    string ipAddress,
    CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        // Ищем клиента по тройке (машина + пользователь + домен)
        var client = await _db.Clients
            .FirstOrDefaultAsync(c =>
                c.MachineName == request.MachineName &&
                c.UserName == request.UserName &&
                c.Domain == request.Domain, ct);

        if (client is null)
        {
            // Новый клиент
            _logger.LogInformation("New client: {Machine}/{User}@{Domain} from {Ip}",
                request.MachineName, request.UserName, request.Domain, ipAddress);

            client = new Client
            {
                MachineName = request.MachineName,
                UserName = request.UserName,
                Domain = request.Domain,
                IpAddress = ipAddress,
                FirstSeenTime = now,
                LastActiveTime = now,
                PendingScreenshotRequest = false
            };
            _db.Clients.Add(client);
        }
        else
        {
            client.LastActiveTime = now;
            client.IpAddress = ipAddress;
        }

        await _db.SaveChangesAsync(ct);

        return new HeartbeatResponse
        {
            ClientId = client.Id,
            TakeScreenshot = false,
            HeartbeatIntervalSeconds = 10
        };
    }

    public async Task<bool> RequestScreenshotAsync(int clientId, CancellationToken ct = default)
    {
        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == clientId, ct);
        if (client is null) return false;

        client.PendingScreenshotRequest = true;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<Screenshot?> SaveScreenshotAsync(
        int clientId,
        Stream fileStream,
        long fileSize,
        CancellationToken ct = default)
    {
        var clientExists = await _db.Clients.AnyAsync(c => c.Id == clientId, ct);
        if (!clientExists) return null;

        var storageRoot = Path.Combine(_env.ContentRootPath, "Storage", "screenshots");
        var clientFolder = Path.Combine(storageRoot, clientId.ToString());
        Directory.CreateDirectory(clientFolder);

        var fileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}.png";
        var fullPath = Path.Combine(clientFolder, fileName);

        await using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await fileStream.CopyToAsync(fs, ct);
        }

        var screenshot = new Screenshot
        {
            ClientId = clientId,
            FilePath = fullPath,
            FileSizeBytes = fileSize > 0 ? fileSize : new FileInfo(fullPath).Length,
            CreatedAt = DateTime.UtcNow
        };

        _db.Screenshots.Add(screenshot);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Screenshot saved: client {ClientId}, {Path}", clientId, fullPath);
        return screenshot;
    }

    public async Task<Screenshot?> GetLatestScreenshotAsync(int clientId, CancellationToken ct = default)
    {
        return await _db.Screenshots
            .Where(s => s.ClientId == clientId)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<Screenshot>> GetAllScreenshotsAsync(int clientId, CancellationToken ct = default)
    {
        return await _db.Screenshots
            .Where(s => s.ClientId == clientId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);
    }
}
