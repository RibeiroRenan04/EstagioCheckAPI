using EstagioCheck.API.Data;
using EstagioCheck.API.DTOs;
using EstagioCheck.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstagioCheck.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "supervisor")]
public class UsersController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAll()
    {
        var users = await db.Users
            .Include(u => u.GroupMembership).ThenInclude(m => m!.Group)
            .OrderBy(u => u.FullName)
            .ToListAsync();

        return Ok(users.Select(u => new UserDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            Matricula = u.Matricula,
            Role = u.Role,
            GroupId = u.GroupMembership?.GroupId,
            GroupCode = u.GroupMembership?.Group?.Code,
            GroupName = u.GroupMembership?.Group?.Name
        }));
    }

    [HttpPatch("{id}/assign-group")]
    public async Task<IActionResult> AssignGroup(Guid id, [FromBody] AssignGroupDto dto)
    {
        var user = await db.Users
            .Include(u => u.GroupMembership)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return NotFound();
        if (user.Role != "aluno")
            return BadRequest(new { message = "Apenas alunos podem ser atribuídos a grupos." });

        // Remove vínculo existente
        if (user.GroupMembership != null)
            db.GroupMemberships.Remove(user.GroupMembership);

        // Novo vínculo
        if (dto.GroupId.HasValue)
        {
            var group = await db.StudentGroups.FindAsync(dto.GroupId.Value);
            if (group == null) return NotFound(new { message = "Grupo não encontrado." });

            db.GroupMemberships.Add(new GroupMembership
            {
                StudentId = id,
                GroupId = dto.GroupId.Value
            });
        }

        await db.SaveChangesAsync();
        return NoContent();
    }
}
