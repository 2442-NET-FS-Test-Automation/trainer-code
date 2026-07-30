using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Library.Tests.Integration;

// Lets test auth over HTTP - this is the same request/response behavior
// you'd get sending these requests from your React front end
[Collection("Library API")]
public class AuthApiTests
{
    
    private readonly HttpClient _client;

    public AuthApiTests(LibraryApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    // The shape that /auth/login returns { "token": "ase4we4234rwafasdf..." }
    private record TokenResponse(string token);

    [Fact]
    public async Task Login_WithSeededAdmin_ReturnsToken()
    {
        // Arrange - but we're just creating a LoginDTO inline - we aren't creating
        // a Mock user or anything like that
        var body = new { username = "ada", password = "pass123!" };

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<TokenResponse>();
        payload!.token.Should().NotBeNullOrWhiteSpace();

    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        // Arrange
        var body = new { username = "ada", password = "wrong-password"};

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

}