using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;
using VidSharePro.Application.Services;
using static System.Net.Mime.MediaTypeNames;

[ApiController]
[Route("api/thumbnails")]
[SupportedOSPlatform("windows")]
public class ThumbnailController(VideoService videoService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetThumbnail(Guid id)
    {
        var filePath = await videoService.GetThumbnailPathAsync(id);

        if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
        {
            // CREATE A DYNAMIC PLACEHOLDER IN MEMORY
            using var bitmap = new Bitmap(320, 180);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.LightGray);
                g.DrawString("Processing...", new System.Drawing.Font("Arial", 12), Brushes.Black, new PointF(100, 80));
            }

            using var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            return File(ms.ToArray(), "image/png");
        }

        return PhysicalFile(filePath, "image/jpeg");
    }
}