namespace WorkActivityMonitor.Models;

public class Screenshot
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public Client Client { get; set; }
    public string FilePath { get; set; }
    public DateTime CreatedAt { get; set; }
}
