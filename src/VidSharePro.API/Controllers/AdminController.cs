// VidSharePro.API/Controllers/AdminController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

[Authorize(Roles = "Admin")] // Simple Role Check
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    [HttpGet("system-stats")]
    public IActionResult GetStats() => Ok(new { status = "Secure" });
}
