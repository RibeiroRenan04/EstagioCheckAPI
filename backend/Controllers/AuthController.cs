using EstagioCheck.API.Data;
using EstagioCheck.API.DTOs;
using EstagioCheck.API.Models;
using EstagioCheck.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstagioCheck.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDbContext db, TokenService tokenService) : ControllerBase
{
    private static readonly string[] AllowedRoles = ["aluno", "preceptor", "supervisor"];

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        if (!AllowedRoles.Contains(dto.Role))
            return BadRequest(new { message = "Papel inválido." });

        var exists = await db.Users.AnyAsync(u => u.Email == dto.Email.ToLower());
        if (exists)
            return Conflict(new { message = "E-mail já cadastrado." });

        var user = new ApplicationUser
        {
            FullName = dto.FullName.Trim(),
            Email = dto.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role,
            Matricula = dto.Matricula?.Trim()
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var token = tokenService.GenerateToken(user);
        return Ok(new AuthResponseDto(token, user.Id.ToString(), user.Email, user.FullName, user.Role));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower());
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Credenciais inválidas." });

        var token = tokenService.GenerateToken(user);
        return Ok(new AuthResponseDto(token, user.Id.ToString(), user.Email, user.FullName, user.Role));
    }
}
