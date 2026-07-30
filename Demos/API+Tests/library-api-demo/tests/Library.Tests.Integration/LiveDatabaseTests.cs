using FluentAssertions;
using Library.Data;
using Library.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Library.Tests.Integration;

// Thankfully cleanup when using EF Core for testing is pretty straightforward
// we have our class Implement IDisposable, and include a one line rollback
// in that method.
public class LiveDatabaseTests: IDisposable
{
    // We need a connection string from either some appsettings file
    // OR from a cloud env - for now we paste it in.
    private const string LiveConnection = 
        "Server=localhost,1433;Database=LibraryMinimalDb;User Id=sa;Password=LibraryPass1!;TrustServerCertificate=true";

    private readonly LibraryDbContext _db;
    private IDbContextTransaction _tx; // this is going to help us with rollback

    public LiveDatabaseTests()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlServer(LiveConnection)
            .Options;

        _db = new LibraryDbContext(options);

        // As xUnit creates the object that will run a test method - we will have it
        // start an EF Core transaction
        _tx = _db.Database.BeginTransaction();

    }

    public void Dispose()
    {
        _tx.Rollback(); // every write/edit done by the test is gone
        _tx.Dispose(); 
        _db.Dispose();
    }

    [Fact]
    public async Task SeedCatalog_IsPresentInTheLiveDatabase()
    {
        // Assert
        // Grab the skus from the DB, not the full object
        var skus = await _db.Products.Select(p => p.Sku).ToListAsync();

        skus.Should().Contain(["BK-001", "BK-002", "BK-003"]);
    }

    [Fact]
    public async Task AddedProduct_IsVisibleTransaction_DeletedUponRollback()
    {
        // Act - write a new product to DB 
        _db.Products.Add(new Product {Sku = "TX-TEST-001", Name = "Rollback Book",
            Price = 1.00m});
        await _db.SaveChangesAsync();

        // Assert - (this should be visible inside this method )
        (await _db.Products.CountAsync(p => p.Sku == "TX-TEST-001")).Should().Be(1);

        // After this method finishes - the transaction is rolled back
        // If that somehow didn't happen - this test would fail. We'd error
        // out trying to add the product to the DB
    }


}