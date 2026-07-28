using FluentAssertions;
using Library.ControllerApi.DTOs;
using Library.ControllerApi.Services;
using Library.Data;
using Library.Data.Entities;
using Moq; 

namespace Library.Tests.Unit;

public class InventoryServiceTests
{
        // Arrange

        // Creating a mock (fake) repo object
        // this way we can avoid calling its actual code, and hitting our DB
        private readonly Mock<IInventoryRepository> _repo = new();

        // Creating an inventory item to serve as default/test data.
        // A little messy because it contains a Product and we create that
        // in line all at once
        private static InventoryItem Item(string sku, string name, int stock) =>
            new() { CurrentStock = stock, 
                Product = new Product { Sku = sku, Name = name, Price = 10m}};


        [Fact]
        public async Task AllAsync_MocksRepository()
    {
        // Arrange - our Mock object sits in for the Data layer - 
        // no database is actually hit
        // Creating a sample list of 1
        var items = new List<InventoryItem> { Item("BK-001", "Clean Code", 5)};

        // When our Mock repo gets a call to it's GetAllAsync method - use the return
        // we configured
        _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(items);
        
        // Using our Mock repo to satisfy the InventoryService constructor
        var sut = new InventoryService(_repo.Object);

        // Act - AllAsync calls the Mock repo's GetAllAsync() who's return
        // we configured above
        var result = await sut.AllAsync();

        // Assert
        // Result should match our items list
        result.Should().BeSameAs(items);
        // GetAllAsync was called ONE time during the Act AllAsync() call
        _repo.Verify(r => r.GetAllAsync(), Times.Once);

    }

    [Fact]
    public async Task AddAsync_UnpacksTheDtoIntoRepoArguments()
    {
        // Arrange
        var dto = new InventoryCreateDto("BK-009", "Domain-Driven Design", 54.99m, 4);

        _repo.Setup(r => r.AddInventoryItemAsync("BK-009", "Domain-Driven Design", 54.99m, 4))
            .ReturnsAsync(Item("BK-009", "Domain-Driven Design", 4));

        var sut = new InventoryService(_repo.Object);

        // Act
        var created = await sut.AddAsync(dto);

        // Assert 
        created.Product.Sku.Should().Be("BK-009");
        _repo.Verify(r => r.AddInventoryItemAsync("BK-009", "Domain-Driven Design", 54.99m, 4)
            , Times.Once);

    }

}