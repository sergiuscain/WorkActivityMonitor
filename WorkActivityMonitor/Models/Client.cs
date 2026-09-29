namespace WorkActivityMonitor.Models;

public class Client
{
    public int Id { get; set; }
    public string MachineName { get; set; }
    public string UserName { get; set; }
    public string Domain { get; set; }
    public string IpAddress { get; set; }
    public DateTime LastActiveTime { get; set; }

    // Навигационное свойство
    public List<Screenshot> Screenshots { get; set; } = new();
}
