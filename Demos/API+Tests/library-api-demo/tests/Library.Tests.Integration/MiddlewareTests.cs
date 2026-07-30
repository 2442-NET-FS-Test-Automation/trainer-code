using System.Net;
using FluentAssertions;

namespace Library.Tests.Integration;

// Middleware testing: Some behavior lives BEFORE controllers and can 
// only be proved through the middleware pipeline. That app.use
// maintenance header Middleware fires before any controller method
// This means no unit tests can see it. 
[Collection("Library API")]
public class MiddlewareTests
{
    private readonly HttpClient _client; 

    public MiddlewareTests(LibraryApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task XMaintenanceHeader_ShortCircuitWith503()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/Inventory");
        request.Headers.Add("X-Maintenance", "1"); // adding our header
        
        // Act
        var response = await _client.SendAsync(request);

        // Assert - the middleware should have redirected this - the controller method
        // should never have ran
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        
    }

}
