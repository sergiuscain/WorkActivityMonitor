namespace WorkActivityMonitor.Dtos;

public class HeartbeatResponse
{
    public int ClientId { get; set; }
    public bool TakeScreenshot { get; set; }
    public int HeartbeatIntervalSeconds { get; set; } = 10;
}
