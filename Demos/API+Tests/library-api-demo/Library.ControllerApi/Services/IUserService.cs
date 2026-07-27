using Library.Data.Entities;

namespace Library.ControllerApi.Services;

public interface IUserService
{
    Task<string?> RegisterAsync(string username, string password);
    Task<User?> ValidateAsync(string username, string password);
}