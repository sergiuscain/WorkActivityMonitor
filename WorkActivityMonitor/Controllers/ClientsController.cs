using Microsoft.AspNetCore.Mvc;

namespace WorkActivityMonitor.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientsController : ControllerBase
{
    /// <summary>Проверка, что контроллер работает.</summary>
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new { status = "ok", timestamp = DateTime.UtcNow });
    }
}
