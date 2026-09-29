namespace WorkActivityMonitor.Dtos;

public class HeartbeatRequest
{
    public string MachineName { get; set; }
    public string UserName { get; set; }
    public string Domain { get; set; }
}
