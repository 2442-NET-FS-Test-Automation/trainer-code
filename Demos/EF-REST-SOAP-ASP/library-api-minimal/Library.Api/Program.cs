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

// App area
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
