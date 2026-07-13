using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Library.ControllerApi.Services;

// Logic for token issuance lives here - any service controller or other service
// that needs a JWT calls this code. 
public class TokenService : ITokenService
{
    private readonly string _key;

    public TokenService(IConfiguration config)
    {
        // We probably want to avoid hardcoding the basis of our key
        // We can always add it to appsettings.Development.json and treat it as a secret.
        // We probably want to then add that file to the .gitignore. Same logic as a .env file
    }
}
