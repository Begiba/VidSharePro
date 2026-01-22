// VidSharePro.FunctionalTests/VideoApiTests.cs
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing; // Fix: Add missing using directive

public class VideoApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public VideoApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Upload_ReturnsUnauthorized_WhenNoTokenProvided()
    {
        var client = _factory.CreateClient();
        var content = new MultipartFormDataContent();

        var response = await client.PostAsync("/api/videos/upload", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}