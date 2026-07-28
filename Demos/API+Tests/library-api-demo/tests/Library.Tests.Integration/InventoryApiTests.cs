using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Library.ControllerApi.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Library.Tests.Integration;

public class InventoryApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    // Arrange
    private readonly HttpClient _client;

    public InventoryApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // The shape that /auth/login returns { "token": "ase4we4234rwafasdf..." }
    private record TokenResponse(string token);

    // I need a method that acts as a helper - it's going to log in as an admin, and return a client
    // that has our bearer token attached
    private async Task<HttpClient> AsAdminAsync()
    {
        var login = await _client.PostAsJsonAsync("/auth/login", new {username = "ada", password = "pass123!"});
        var token = (await login.Content.ReadFromJsonAsync<TokenResponse>())!.token;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return _client;
    }


    [Fact]
    public async Task GetInventory_ContainsTheSeededCatalog()
    {
        // Arrange 

        // Act
        var items = await _client.GetFromJsonAsync<List<InventoryDto>>("/api/Inventory");

        // Assert
        items.Should().NotBeNullOrEmpty();
        items.Select(i => i.Sku).Should().Contain(["BK-001", "BK-002", "BK-003"]);
    }

    [Fact]
    public async Task GetBySku_UnknownSku_Returns404()
    {
        // Act /api/Inventory/BK-001
        var response = await _client.GetAsync("/api/Inventory/SOME-NONSENSE");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

    }

    [Fact]
    public async Task PostInventory_WithoutToken_Returns401()
    {
        // Arrange - a perfectly valid body - client is just missing the token inside the headers
        var dto = new InventoryCreateDto("BK-401", "No Token", 10.00m, 1);

        // Act 
        var response = await _client.PostAsJsonAsync("/api/Inventory", dto);

        // Assert - the [Authorize(Roles = "admin")] annotation should fire before any controller
        // code runs, sending a response with a 401 Unauthorized
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

    }

    [Fact]
    public async Task PostInventory_AsAdmin_Creates()
    {
        // Arrange 
        // Using the modified client that comes back from our helper method - that has the Admin token
        var client = await AsAdminAsync();
        var dto = new InventoryCreateDto("BK-TEST-INT", "Integration Test Book", 21.50m, 2);

        // Act - this will actually create something on the database
        var created = await client.PostAsJsonAsync("/api/Inventory", dto);

        // Assert - we should have a 201 status code  with a Location header
        // that points to the new resource
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        created.Headers.Location!.ToString().Should().Contain("BK-TEST-INT");

        // Cleanup - VERY IMPORTANT IN INTEGRATION TESTING (and really anything above unit testing)
        // This test creates data in our database. It would at best, clutter up our DB, at worst
        // poison future tests
        var deleted = await client.DeleteAsync("/api/Inventory/BK-TEST-INT");
        deleted.StatusCode.Should().Be(HttpStatusCode.NoContent);

    }

}