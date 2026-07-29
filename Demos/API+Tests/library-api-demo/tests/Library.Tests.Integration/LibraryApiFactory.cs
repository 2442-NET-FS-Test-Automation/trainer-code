using Library.ControllerApi.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Library.Tests.Integration;

// Our tests use an in memory server. WebApplicationFactory<Program> boots 
// our ENTIRE Library.ControllerApi - its real Program.cs, real middleware pipeline
// real EF Core, etc. The "server" lives inside the test process. CreateClient()
// hands back an HttpClient that is wired straight into it: no port, no network,
// it hits it in memory. 

// Program here is the API's entry point - Program.cs in Library.ControllerAPI. 
public class LibraryApiFactory : WebApplicationFactory<Program>
{
    // We are going to fake ONE thing in our entire integration test suite
    // the call to the dummyjson.com api. 
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // What we are doing in here is appending to the builder.services section
            // of the API's Program.cs. We can replace, add, override, etc. 
            services.AddSingleton<ISupplierClient, FakeSupplierClient>();
        });
    }
}

// Deterministic stand-in (fake) for the external supplier API
public class FakeSupplierClient : ISupplierClient
{
    public Task<decimal?> GetListPriceAsync(string sku)
    {
        // We provide a hard coded return
        return Task.FromResult<decimal?>(99.99m);
    }
}