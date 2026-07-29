using FluentAssertions;
using Library.ControllerApi.Services;
using Library.Data;
using Library.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

public class UserServiceSqliteTests : IDisposable
{

    // SQLite has limits that keep it from being 1:1 with SqlServer
    // It has no idea what a RowVersion is - so the seed data in OnModelCreating
    // causes it to throw an error. We can create a context sublass that gives
    // a default value that Sqlite can accept for the the column in every row. 
    // This is that trade off Microsoft warned us about - SQLite is SQL, but it 
    // isn't SQl Server
    private class SqliteLibraryDbContext : LibraryDbContext
    {
        public SqliteLibraryDbContext(DbContextOptions<LibraryDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);
            b.Entity<InventoryItem>().Property(i => i.RowVersion)
                .HasDefaultValue(Array.Empty<byte>());
        }
    }



    private readonly SqliteConnection _connection;
    private readonly LibraryDbContext _db;
    private readonly UserService _sut; // the thing we're testing

    public UserServiceSqliteTests()
    {
        // Setting up for our sqlite db
        // Our sqlite db will be held in memory and disposed of when the test is done
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open(); 

        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new SqliteLibraryDbContext(options);
        _db.Database.EnsureCreated(); // builds the schema from our models. No migration.

        // Finally we can make our UserService object
        _sut = new UserService(_db, new PasswordHasher<User>());

    }

    public void Dispose()
    {
        _db.Dispose(); // Dispose of the library db context - close the connection
        _connection.Dispose(); // delete the in-memory db itself
    }

    [Fact]
    public async Task RegisterAsync_NewUser_PersistsAHashedPassword()
    {
        // Act
        var error = await _sut.RegisterAsync("grace", "s3cure-pass!");

        // Assert - null means success (the service's contract)
        error.Should().BeNull();
        var saved = await _db.Users.SingleAsync(u => u.Username == "grace");
        saved.Role.Should().Be("consumer");           // never trust the client with the role
        saved.PasswordHash.Should().NotBeNullOrEmpty();
        saved.PasswordHash.Should().NotBe("s3cure-pass!"); // hashed, NEVER plaintext
    }

    [Fact]
    public async Task RegisterAsync_DuplicateUsername_ReturnsTakenMessage()
    {
        // Arrange
        await _sut.RegisterAsync("grace", "first-pass!");

        // Act - same name, different padding: the service trims before comparing
        var error = await _sut.RegisterAsync("  grace  ", "second-pass!");

        // Assert
        error.Should().Be("username is taken");
        (await _db.Users.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task ValidateAsync_WrongPassword_ReturnsNull()
    {
        // Arrange
        await _sut.RegisterAsync("grace", "right-pass!");

        // Act
        var user = await _sut.ValidateAsync("grace", "wrong-pass!");

        // Assert - and an unknown username gives the same null: no user enumeration
        user.Should().BeNull();
        (await _sut.ValidateAsync("grace", "right-pass!")).Should().NotBeNull();
    }
}