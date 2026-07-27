using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Library.ControllerApi.Services;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

// Putting my unit tests in one namespace - can also separate them 
// based on test domain (Inventory vs Auth vs User, etc)
namespace Library.Tests.Unit;

// Let's start with TokenService - smallest Service to test from our demo API
// One dependency (IConfiguration), one method (Issue() ), and a pure
// deterministic output - the JWT string
public class TokenServiceTests
{

    private readonly ITestOutputHelper _output;

    // Arrange
    // Same key shape the API uses - but a different seed string
    // Still needs to be 32 bytes or more for HS256 encoding
    private const string TestKey = "unit-test-signing-key-32-bytes-min!!";

    // A constructor for our Test class - uses DI like ASP.NET
    public TokenServiceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    // If we have something like a Service class who's methods we are testing,
    // we need to "build" the System Under Test (SUT) - for us - that's an actual
    // object of type TokenService
    private static TokenService CreateSut()
    {   
        // Using an InMemoryCollection to satisfy the config, rather than 
        // reading appsettings.json 
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
                { ["Jwt:key"] = TestKey })
            .Build();

        // Calling TokenService's actual constructor
        return new TokenService(config);
    }

    // Our first test method
    // Follows the Method_ConditionWeAreTestingFor syntax
    [Fact]
    public void Issue_ReturnsParsableJwt()
    {
        // Arrange
        // creating our TokenService object
        var sut = CreateSut();

        // Act
        var token = sut.Issue("ada", "admin");
        _output.WriteLine(token);
        
        // Assert
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Using fluent assertions - lets us write asserts that sound 
        // like english
        parsed.Issuer.Should().Be("library-fulfillment");
        parsed.Audiences.Should().Contain("library-fulfillment-clients");

        // The base assert functionality in xunit looks like the following
        Assert.Equal("library-fulfillment", parsed.Issuer);
        Assert.Contains("library-fulfillment-clients", parsed.Audiences);

    }

    // Let's write a test case to make sure we are getting Name/Role
    // info encoded in our JWT's Claims
    [Fact]
    public void Issue_IncludesNameAndRoleClaims()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var token = sut.Issue("ada", "admin");

        // Assert    
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // We have to use the full URI claim names - because that's what my front end
        // expects. If someone comes in and configures shorter claim names - the test
        // fails before the SPA login breaks later
        parsed.Claims.Should().Contain(c => 
            c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" 
                && c.Value == "ada");

        parsed.Claims.Should().Contain(c => 
            c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                && c.Value == "admin");
    }

    // Fact tests are test methods that take no inputs
    // Theory tests take input. We can provide 1 or more sets of input data
    // to our test, to test for positive and negative conditions if we want/need to
    // each set of input data counts as a SEPARATE test
    [Theory]
    [InlineData("ada", "admin")]
    [InlineData("grace", "consumer")]
    public void Issue_SetsRoleClaim_ForAnyRole(string user, string role)
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var token = sut.Issue(user, role);

        // Assert    
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

        parsed.Claims.Should().Contain(c => 
            c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                && c.Value == role);

    }
}