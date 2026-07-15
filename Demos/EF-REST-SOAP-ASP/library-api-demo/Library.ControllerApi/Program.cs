using System.Text;
using Library.ControllerApi.Filters;
using Library.ControllerApi.Mapping;
using Library.ControllerApi.Middleware;
using Library.ControllerApi.Services;
using Library.Data;
using Library.Data.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Adding connection string 
var conn_string = builder.Configuration.GetConnectionString("Library") ?? 
    "Server=localhost,1433;Database=LibraryMinimalDb;User Id=sa;Password=LibraryPass1!;TrustServerCertificate=true";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console() // Write to console, and write to a file - starting a new file each day.
    .WriteTo.File("logs/fulfillment-log-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog(); // Tell the builder to use Serilog for logging

// Adding CORS 
const string SpaCorsPolicy = "spa"; // string name for our policy 

// Configuring our CORS policy
builder.Services.AddCors(o => o.AddPolicy(SpaCorsPolicy, p => p
    .WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
    .AllowAnyHeader()
    .AllowAnyMethod()
));


// VALIDATION side of JWT. Issuance lives in TokenService
var jwtKey = builder.Configuration["Jwt:Key"]; //from appsettings.Development.json

// Hardcoding the issuer and audience - these have to match the ones we set on the token
const string jwtIssuer = "library-fulfillment";
const string jwtAudience = "library-fulfillment-clients";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters
    {
       ValidateIssuer = true, ValidIssuer = jwtIssuer,
       ValidateAudience = true, ValidAudience = jwtAudience,
       ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
       ValidateLifetime = true
    });

builder.Services.AddAuthorization(); // goes after authentication

// Token Issuance is a plain injectable service. Its stateless so we can use a singleton
builder.Services.AddSingleton<ITokenService, TokenService>();

// Adding our HttpClient
builder.Services.AddHttpClient<ISupplierClient, SupplierClient>(c => 
    c.BaseAddress = new Uri("https://dummyjson.com/") // all calls append to this URL
);


builder.Services.AddDbContextFactory<LibraryDbContext>(o => o.UseSqlServer(conn_string));

// Adding the password hasher
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

// Registering our custom Repo and Service Layer methods like we did before
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>(); // Could later swap for InventoryMongoRepo
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IUserService, UserService>();

// Adding our mapping profile for AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(MappingProfile).Assembly));

// having our filter apply to every controller
builder.Services.AddControllers(o => o.Filters.Add<TimingFilter>());

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Adding swagger back
builder.Services.AddSwaggerGen();

// Adding caching
builder.Services.AddMemoryCache(); // adding cache-ing to our server
builder.Services.AddResponseCaching(); // adding response cache-ing - asking the front end to save request results 


var app = builder.Build();

// Seeding admins - cant do a plain INSERT INTO using SQL because I won't have a hashed password
// might be able to do it in LibraryDbContext - would have to check how to do that.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

    // We want this to be idempotent. This block of code runs EVERY time the app starts
    // BUT we only want to seed our admin(s) ONCE. 
    if (!db.Users.Any(u => u.Role == "admin"))
    {
        var hasher = new PasswordHasher<User>();
        var admin = new User { Username = "ada", Role = "admin"};

        // I should put that password inside of some secret (non GH committed) file. 
        admin.PasswordHash = hasher.HashPassword(admin, "pass123!"); // put this in a config file pls!

        db.Users.Add(admin);
        db.SaveChanges();
    }

}


app.UseMiddleware<ExceptionHandlingMiddleware>(); // wraps all middleware below it, catches their exceptions

// Swagger stuff added to app
app.UseSwagger();
app.UseSwaggerUI();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// This is a simple diagnostic middleware. All it will do is time our requests for us and log that.
// It takes in ctx (HttpContext -> everything about the request AND the response) 
// next - represents a call to the subsequent middleware
app.Use(async (ctx, next) =>
{
    var sw = System.Diagnostics.Stopwatch.StartNew();

    await next(ctx); // calling the next middleware in the chain - whatever that is

    sw.Stop();
    Log.Information("{Method} {Path} -> {StatusCode} in {Elapsed} ms",
        ctx.Request.Method, ctx.Request.Path, ctx.Response.StatusCode, sw.ElapsedMilliseconds
    );
});

app.Use(async (ctx, next) =>
{
    if (ctx.Request.Headers.ContainsKey("X-Maintenance"))
    {
        ctx.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        await ctx.Response.WriteAsync("Down for maintenance");
        return; // don't call next() - never hits controllers
    }

    await next(ctx);
});

app.UseCors(SpaCorsPolicy); //using our policy, with the CORS middleware
// CORS must run BEFORE response caching: a cached reply seeded by a request with no
// Origin header would otherwise replay for 30s WITHOUT the Access-Control-Allow-Origin
// header, and the browser fails the page's fetch intermittently.

app.UseResponseCaching(); // using the response cache middleware

//Must be in this order for Authn/Authz
app.UseAuthentication(); // read and validate the tokens -> set User
app.UseAuthorization(); // enforces the [Authorize] / RequireAuthorization() decorators on endpoints 

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

Log.CloseAndFlush(); // Close and flush the logs (serilog)
