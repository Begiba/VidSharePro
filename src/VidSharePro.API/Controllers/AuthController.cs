// VidSharePro.API/Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using VidSharePro.Application.DTOs;
using VidSharePro.Application.Services;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        // AuthService handles validation and password verification
        var response = await authService.LoginAsync(request, HttpContext.RequestAborted);
        return Ok(response);
    }
}