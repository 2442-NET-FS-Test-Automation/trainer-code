using Microsoft.EntityFrameworkCore;
using Library.Data;

// This is my API program.cs
// No main. We can think of it as 2 sections
// Registering things with the builder. 
// And then configuring things on the app
// And at the very bottom that app object that represents our entire API calls its run method

// Builder area
var builder = WebApplication.CreateBuilder(args);

// The first thing that we need is to give our builder a connection string to our database
var conn_string = "Server=localhost,1433;Database=LibraryMinimalDb;User Id=sa;Password=LibraryPass1!;TrustServerCertificate=true";

// Tell the builder to use our LibraryDbContext with the connection string above
// By registering our DbContext class (or even clases, technically you use one per Database)
// we hand off the managing of creating and destroying these DbContext objects to ASP.NET's
// dependency injection container. Like spring beans if you're familiar. 
builder.Services.AddDbContext<LibraryDbContext>(options => options.UseSqlServer(conn_string)); 

// Swagger stuff added to builder
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// App area
var app = builder.Build();

// Swagger stuff added to app
app.UseSwagger();
app.UseSwaggerUI();

// Endpoint area
app.MapGet("/", () => "Hello World!");

// Get all items from the inventory
app.MapGet("/inventory", async (LibraryDbContext db) => {
    // we should probably await this - may not matter because we are local
    return await db.Inventory.ToListAsync();
});

// Lets use LINQ - Language Integrated Query
// LINQ is a library that just lets us query collections
// The logic actually flows from SQL DQL - You can use method OR sql query syntax
// You can even save the queries themselves as C# objects if you want to 
app.MapGet("/inventory/by-value", (LibraryDbContext db) => {
    
    return db.Inventory.Include(i => i.Product)
        .GroupBy(i => i.CurrentStock >= 5 ? "well-stocked": "low")  //group by just like in sql
        .Select(g => new { tier = g.Key, count = g.Count(), units = g.Sum(i => i.CurrentStock) })
        .ToList();
});


// My file always ends with app.Run() - minimal API or Controller API
app.Run();