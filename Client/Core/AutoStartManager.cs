using Microsoft.Win32;

namespace Client.Core
{
    internal static class AutoStartManager
    {
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "WorkActivityMonitor";

        public static bool IsEnabled()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: false);
            return key?.GetValue(AppName) != null;
        }

        public static void Enable()
        {
            if (IsEnabled())
            {
                return;
            }

            var exePath = Environment.ProcessPath;
            if (string.IsNullOrEmpty(exePath))
            {
                SimpleLogger.Log("AutoStart: failed to resolve exe path");
                return;
            }

            using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
            if (key is null)
            {
                SimpleLogger.Log("AutoStart: Run key not found");
                return;
            }

            key.SetValue(AppName, $"\"{exePath}\"");
            SimpleLogger.Log($"AutoStart enabled: {exePath}");
        }
    }
}