using System.ComponentModel.DataAnnotations;

namespace Library.ControllerApi.DTOs;

// You probably want separate login and register DTOs
// based on what info you have users provide when they register

public record RegisterDto(
    [Required, MaxLength(64)] string Username,
    [Required, MinLength(8)] string Password
    // could ask for phone number, email, etc as well
);

public record loginDto(
    [Required] string Username,
    [Required] string Password
);