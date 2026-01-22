// VidSharePro.API/Controllers/MetricsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("metrics")]
public class MetricsController(IMetricsService metrics) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin")] // Protect metrics from public view
    public IActionResult Get() => Ok(metrics.GetCurrentMetrics());
}