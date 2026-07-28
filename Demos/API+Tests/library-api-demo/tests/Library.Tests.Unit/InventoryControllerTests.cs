using AutoMapper;
using FluentAssertions;
using Library.ControllerApi.Mapping;
using Library.ControllerApi.Services;
using Library.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Library.Tests.Unit.Fixtures;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Library.Tests.Unit;

// Wiring our class to use the IClassFixture for our MapperFixture
public class InventoryControllerTests : IClassFixture<MapperFixture>
{
    // InventoryController has a few dependencies 
    private readonly Mock<IInventoryService> _service = new();
    private readonly Mock<ISupplierClient> _supplier = new();
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly IMapper _mapper; // we don't mock our mapper, 
    // we need its actual mapping behavior (AutoMapper)

    // We need a constructor for our InventoryControllerTests, 
    // because we need to do a little config for the mapper
    public InventoryControllerTests(MapperFixture mFixture)
    {
        // var config = new MapperConfiguration(cfg => 
        //     cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance);
        _mapper = mFixture.Mapper;
    }

    // Method to call the InventoryController constructor and pass it our 
    // Mock objects + mapper
    private InventoryController CreateSut() => 
        new(_service.Object, _mapper, _cache, _supplier.Object);

    private static InventoryItem Item(string sku, string name, int stock) =>
        new() { CurrentStock = stock, 
            Product = new Product { Sku = sku, Name = name, Price = 10m}};

    
    [Fact]
    public async Task Get_ReturnsOkWithMappedDtos()
    {
        // Arrange
        _service.Setup(s => s.AllAsync())
            .ReturnsAsync(new List<InventoryItem> { Item("BK-001", "Clean Code", 5)});
        
        var sut = CreateSut();
        
        // Act
        var result = await sut.Get();

        // Assert 
        // .Subject lets us extract the object we just made an assertion about - so we can reuse 
        // or make further assertions later - like we did below. 
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(new [] { new { Sku = "BK-001", Name = "Clean Code", CurrentStock = 5}});
    }

    // Lets test the cache-ing behavior
    [Fact]
    public async Task Get_SecondCall_ServesFromCache_ServiceCalledOnce()
    {
        // Arrange
        _service.Setup(s => s.AllAsync())
            .ReturnsAsync(new List<InventoryItem> { Item("BK-001", "Clean Code", 5)});

        var sut = CreateSut();  
        
        // Act - two requests, same controller, same cache
        await sut.Get();
        await sut.Get();

        // Assert
        _service.Verify(s => s.AllAsync(), Times.Once);

    }

     [Fact]
    public async Task GetBySku_UnknownSku_ReturnsNotFound()
    {
        // Arrange - loose mock default would return null anyway; being explicit reads better.
        _service.Setup(s => s.BySkuAsync("NOPE")).ReturnsAsync((InventoryItem?)null);
        var sut = CreateSut();

        // Act
        var result = await sut.GetBySku("NOPE");

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetSupplierPrice_ReturnsClientPrice()
    {
        // Arrange - no real HTTP call to the supplier; the mock answers instantly.
        _supplier.Setup(s => s.GetListPriceAsync("BK-001")).ReturnsAsync(12.34m);
        var sut = CreateSut();

        // Act
        var result = await sut.GetSupplierPrice("BK-001");

        // Assert - BeEquivalentTo compares by shape, so the controller's anonymous object matches ours.
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(new { sku = "BK-001", supplierPrice = 12.34m });
    }

}