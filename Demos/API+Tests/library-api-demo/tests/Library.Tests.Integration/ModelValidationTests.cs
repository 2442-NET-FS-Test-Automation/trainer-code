using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Library.ControllerApi.DTOs;
using Library.Tests.Integration;

namespace Library.Tests.Integration;

// We want to test things about our Models - namely, do they validate 
// data we try to give them (min and max value, max length of a string, etc)
// we can also test to make sure that ASP.NET model validation is enforcing those 
// rules as well 
[Collection("Library API")]
public class ModelValidationTest
{
    private readonly HttpClient _client;

    public ModelValidationTest(LibraryApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private record TokenResponse(string token);

    // First - a test to show off WHEN validation actually happens
    // more for demo - not a practical test
    [Fact]
    public void DirectValidator_MissesPositionalRecordAttributes()
    {
        // Arrange - A negative price violates [Range(0.01, 100000)]
        var dto = new InventoryCreateDto("BK_BAD", "Bad Book", -50.00m, 1);
        var results = new List<ValidationResult>(); 

        // Act
        var valid = Validator.TryValidateObject(dto, new ValidationContext(dto),
            results, validateAllProperties: true);

        // Assert - this is going to seem backwards which is why its demo only
        valid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task PostInventory_WithInvalidBody_Returns400()
    {
        // Arrange - auth comes FIRST in our pipeline, and we need to be authenticated
        // in order to do this
        var login = await _client.PostAsJsonAsync("/auth/login",
             new {username = "ada", password = "pass123!"});
        var token = (await login.Content.ReadFromJsonAsync<TokenResponse>())!.token;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var dto = new InventoryCreateDto("BK_BAD", "Bad Book", -50.00m, 1);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Inventory", dto);

        // Assert = [ApiController]'s model validation should send back a 400 before the controller
        // method ever runs
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
    }
}