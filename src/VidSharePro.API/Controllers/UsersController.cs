// VidSharePro.API/Controllers/UsersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VidSharePro.Application.DTOs;
using VidSharePro.Application.Services;
using VidSharePro.Domain.Entities;

[ApiController]
[Route("api/[controller]")]
public class UsersController(UserService userService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        await userService.RegisterAsync(request, HttpContext.RequestAborted);
        return StatusCode(StatusCodes.Status201Created);
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentProfile()
    {
        return Ok(new
        {
            Username = User.Identity?.Name,
            Role = User.FindFirstValue(ClaimTypes.Role)
        });
    }
}