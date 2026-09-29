
namespace Client;

internal class TrayApplicationContext : ApplicationContext
{
    private readonly System.Windows.Forms.Timer _timer;
    public TrayApplicationContext()
    {
        SimpleLogger.Log("Application started");

        _timer = new System.Windows.Forms.Timer { Interval = 10_000 }; // 10 секунд
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        SimpleLogger.Log("Tick");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer?.Stop();
            _timer?.Dispose();
        }
        base.Dispose(disposing);
    }
}
