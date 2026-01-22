// VidSharePro.API/Controllers/VideosController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Security.Claims;
using VidSharePro.API.Attributes;
using VidSharePro.API.Models;
using VidSharePro.Application.Common.Configuration;
using VidSharePro.Application.Common.Interfaces;
using VidSharePro.Application.DTOs;
using VidSharePro.Application.Services;
using VidSharePro.Domain.Entities;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class VideosController(
    VideoService videoService,
    IVideoRepository videoRepository,
    IOptions<StorageOptions> storageOptions) : ControllerBase
{
    private readonly string _storagePath = storageOptions.Value.Path;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // Fetch the user ID from the JWT token
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();

        var userId = Guid.Parse(userIdString);
        var videos = await videoService.GetUserVideosAsync(userId, HttpContext.RequestAborted);
        return Ok(videos);
    }

    [Authorize]
    [EnableRateLimiting("uploading")]
    [HttpPost("upload")]    
    [RequestSizeLimit(1_073_741_824)]
    [DisableFormValueModelBinding]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload()
    {
        var syncIOFeature = HttpContext.Features.Get<IHttpBodyControlFeature>();
        if (syncIOFeature != null) syncIOFeature.AllowSynchronousIO = true;

        var title = Request.Form["Title"];
        var file = Request.Form.Files.GetFile("File");

        if (file == null)
        {
            ModelState.AddModelError("File", "File is required.");
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(errors);
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(errors);
        }

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        using var stream = file.OpenReadStream();
        var dto = new UploadVideoRequestDto(
            Title : title + "",
            file.FileName,
            file.Length,
            file.ContentType,
            stream
        );

        var videoId = await videoService.UploadVideoAsync(userId, dto, HttpContext.RequestAborted);
        return Ok(new { VideoId = videoId });
    }

    // Remove both old [HttpGet("stream/{id}")] methods and use this one:
    [HttpGet("{id}/stream")]
    [AllowAnonymous] // Access is managed via the token query parameter
    public async Task<IActionResult> Stream(Guid id, [FromQuery] string? token)
    {
        // 1. Basic Validation
        if (string.IsNullOrEmpty(token)) return Unauthorized("Token is required.");

        var video = await videoRepository.GetByIdAsync(id);
        if (video == null) return NotFound();

        // 2. Construct the path
        var filePath = Path.Combine(_storagePath, video.StoragePath);

        // 3. Physical File Check
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 1024 * 64,
            useAsync: true
        );

        // 4. Return the stream with Range Processing enabled for the seek bar
        //return PhysicalFile(filePath, "video/mp4", enableRangeProcessing: true);
        return File(
            stream,
            "video/mp4",
            enableRangeProcessing: true
        );
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await videoService.SoftDeleteVideoAsync(id, HttpContext.RequestAborted);
        return NoContent();
    }
}