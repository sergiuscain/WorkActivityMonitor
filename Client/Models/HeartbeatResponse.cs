using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Models;

internal class HeartbeatResponse
{
    public int ClientId { get; set; }
    public bool TakeScreenshot { get; set; }
    public int HeartbeatIntervalSeconds { get; set; }
}
