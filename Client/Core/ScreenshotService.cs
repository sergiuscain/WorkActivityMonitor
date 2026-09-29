using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Client.Core
{
    internal static class ScreenshotService
    {
        /// <summary>Захватывает весь основной экран и возвращает PNG в виде байтов.</summary>
        public static byte[] CaptureScreen()
        {
            var bounds = Screen.PrimaryScreen!.Bounds;

            using var bitmap = new Bitmap(bounds.Width, bounds.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
            }

            using var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }

        /// <summary>Сохраняет байты PNG во временную папку и возвращает путь к файлу.</summary>
        public static string SaveToTempFile(byte[] pngBytes, int clientId)
        {
            var dir = Path.Combine(AppContext.BaseDirectory, "screenshots");
            Directory.CreateDirectory(dir);

            var fileName = $"client_{clientId}_{DateTime.Now:yyyyMMdd_HHmmss_fff}.png";
            var path = Path.Combine(dir, fileName);
            File.WriteAllBytes(path, pngBytes);
            return path;
        }
    }
}