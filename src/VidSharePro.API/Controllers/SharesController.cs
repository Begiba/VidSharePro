// VidSharePro.API/Controllers/SharesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VidSharePro.Application.DTOs;
using VidSharePro.Application.Services;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SharesController(ShareService shareService) : ControllerBase
{
    [HttpPost("generate")]
    public async Task<IActionResult> CreateShare([FromBody] CreateShareRequestDto request)
    {
        var token = await shareService.CreateShareLinkAsync(
            request.VideoId,
            request.ExpiryDate,
            HttpContext.RequestAborted
        );

        return Ok(new { ShareToken = token, Link = $"/v/{token}" });
    }
}