using Library.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Adding connection string 
var conn_string = "Server=localhost,1433;Database=LibraryMinimalDb;User Id=sa;Password=LibraryPass1!;TrustServerCertificate=true";

builder.Services.AddDbContextFactory<LibraryDbContext>(o => o.UseSqlServer(conn_string));

// Registering our custom Repo and Service Layer methods like we did before
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>(); // Could later swap for InventoryMongoRepo

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Adding swagger back
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger stuff added to app
app.UseSwagger();
app.UseSwaggerUI();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
