using Library.ControllerApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Library.ControllerApi.DTOs;

[ApiController]
[Route("auth")] // localhost:5137/auth/endpoints/etc
public class AuthController : ControllerBase
{
    // Same constructor injection as any other controller. The token stuff is just another
    // service behind and interface
    private readonly ITokenService _tokens;
    private readonly IUserService _users;

    public AuthController (ITokenService tokens, IUserService users)
    {
        _tokens = tokens;
        _users = users;
    }

    // POST to register a user
    // POST /auth/register {username, password} -> 201
    // ONLY creates consumer accounts, admins should be seeded 
    // do not make a register as admin endpoint visible to users
    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterDto dto)
    {
        var error = await _users.RegisterAsync(dto.Username, dto.Password);

        if (error is not null)
        {
            return Conflict(new { error }); // 409 Conflict - duplicate username 
        }

        return CreatedAtAction(nameof(Me), null); // 201; this will make use of another endpoint for its return

    }

    // POST /auth/login {username, password} -> returns a token
    [HttpPost("login")]
    public async Task<ActionResult> Login(loginDto dto)
    {
        var user = await _users.ValidateAsync(dto.Username, dto.Password);

        if (user is null)
        {
            return Unauthorized(new { error = "bad credentials" }); //401 - same for bad pass/username
        }


        // Username and Role come from the FOUND USER - not the loginDTO
        return Ok(new { token = _tokens.Issue(user.Username, user.Role) });

    }

    // GET /auth/me - reads the Claims. The authentication middleware validates and parses the token
    // into User on the way in. We will read UserId from the claim on the token - never from a query string
    [HttpGet("me")]
    public ActionResult Me()
    {
        return Ok( new
        {
            name = User.Identity?.Name,
            role = User.FindFirstValue(ClaimTypes.Role)
        });
    }


    // Logins are always a POST - since you are sending user credentials in the body of the request
    // We aren't set up to track user accounts as a DB Table YET - we will add that in the future
    // For now, we are going to skip verifying credentials (ask DB for a user object and compare, etc)
    // and we'll just issue the token
    [HttpPost("token")]
    public ActionResult IssueToken(string userName)
    {
        // Get a new token
        var userToken = _tokens.Issue(userName, "consumer");

        // Return it
        return Ok(userToken);
    }

}