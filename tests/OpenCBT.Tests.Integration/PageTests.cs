using FluentAssertions;
using System.Net;

namespace OpenCBT.Tests.Integration;

public class PageTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PageTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Get_HomePage_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.EnsureSuccessStatusCode(); 
        response.Content.Headers.ContentType?.ToString().Should().Contain("text/html");
    }

    [Fact]
    public async Task Get_ExamsPage_WithoutAuth_RedirectsToLogin()
    {
        // Act
        var response = await _client.GetAsync("/Exams");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Contain("/Account/Login");
    }
}
