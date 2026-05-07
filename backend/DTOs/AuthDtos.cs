using System.ComponentModel.DataAnnotations;

namespace EstagioCheck.API.DTOs;

public record RegisterDto(
    [Required, MinLength(2), MaxLength(200)] string FullName,
    [Required, EmailAddress, MaxLength(255)] string Email,
    [Required, MinLength(6), MaxLength(100)] string Password,
    [MaxLength(50)] string? Matricula,
    [Required] string Role  // "aluno" | "preceptor" | "supervisor"
);

public record LoginDto(
    [Required, EmailAddress] string Email,
    [Required] string Password
);

public record AuthResponseDto(
    string Token,
    string UserId,
    string Email,
    string FullName,
    string Role
);
