using System;
using System.Collections.Generic;
using System.Text;

namespace Client;

internal class SimpleLogger
{
    private static readonly object _lock = new();
    private static readonly string _logDir = Path.Combine(AppContext.BaseDirectory, "logs");
    private static readonly string _logPath = Path.Combine(_logDir, "client.log");

    public static void Log(string message)
    {
        lock (_lock)
        {
            Directory.CreateDirectory(_logDir);
            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
            File.AppendAllText(_logPath, line);
        }
    }
}
