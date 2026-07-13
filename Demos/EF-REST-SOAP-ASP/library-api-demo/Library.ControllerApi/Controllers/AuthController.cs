using Library.ControllerApi.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("auth")] // localhost:5137/auth/endpoints/etc
public class AuthController : ControllerBase
{
    // Same constructor injection as any other controller. The token stuff is just another
    // service behind and interface
    private readonly ITokenService _tokens;

    public AuthController (ITokenService tokens)
    {
        _tokens = tokens;
    }

    // Logins are always a POST - since you are sending user credentials in the body of the request
    // We aren't set up to track user accounts as a DB Table YET - we will add that in the future
    // For now, we are going to skip verifying credentials (ask DB for a user object and compare, etc)
    // and we'll just issue the token
    [HttpPost("token")]
    public ActionResult IssueToken(string userName)
    {
        // Get a new token
        var userToken = _tokens.Issue(userName);

        // Return it
        return Ok(userToken);
    }

}