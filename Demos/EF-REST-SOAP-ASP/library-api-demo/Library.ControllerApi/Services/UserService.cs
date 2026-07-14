using Library.Data;
using Library.Data.Entities;
using Microsoft.AspNetCore.Identity; // not the full framework - we just need the PasswordHasher
using Microsoft.EntityFrameworkCore;

namespace Library.ControllerApi.Services;

public class UserService : IUserService
{
    private readonly LibraryDbContext _db;

    // Comes from ASP.NET Identity. Uses per-password salt to obfuscate/hash passwords
    // we will hash THEN store. And always verify against that hash. Never store plaintext passwords.
    // Generally: don't invent your own hashing
    private readonly IPasswordHasher<User> _hasher;

    public UserService(LibraryDbContext db, IPasswordHasher<User> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task<string?> RegisterAsync(string username, string password)
    {
        //first - trim the string
        string name = username.Trim();

        // Check to see if the username is already taken
        if (await _db.Users.AnyAsync(u => u.Username == name))
        {
            return "username is taken";
        }

        User newUser = new User { Username = name, Role = "consumer" }; // never trust client on the role

        // Hashing + salting password - uses the newUser object + password
        newUser.PasswordHash = _hasher.HashPassword(newUser, password);

        _db.Users.Add(newUser);
        await _db.SaveChangesAsync();
        return null; // if all goes well - we return null 

    }

    public async Task<User?> ValidateAsync(string username, string password)
    {
        User? foundUser = await _db.Users.SingleOrDefaultAsync(u => u.Username == username);

        if (foundUser is null) return null; // unknown username and wrong pass look IDENTICAL
        // probably not the best implementation - you guys can do more checks later. 

        // Using the hasher to verify a hashed password. 
        var result = _hasher.VerifyHashedPassword(foundUser, foundUser.PasswordHash, password);

        return result == PasswordVerificationResult.Failed ? null : foundUser;

    }
}