using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Models;

internal class HeartbeatRequest
{
    public string MachineName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
}
