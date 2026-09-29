namespace WorkActivityMonitor.Dtos;

public class ClientDto
{
    public int Id { get; set; }
    public string MachineName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime LastActiveTime { get; set; }
    public bool IsOnline { get; set; }
    public int ScreenshotCount { get; set; }
}
