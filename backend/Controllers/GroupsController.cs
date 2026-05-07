using EstagioCheck.API.Data;
using EstagioCheck.API.DTOs;
using EstagioCheck.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstagioCheck.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GroupsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<GroupDto>>> GetAll()
    {
        var groups = await db.StudentGroups
            .Include(g => g.Memberships)
            .OrderBy(g => g.Code)
            .ToListAsync();

        return Ok(groups.Select(g => new GroupDto
        {
            Id = g.Id, Code = g.Code, Name = g.Name, Description = g.Description,
            MemberCount = g.Memberships.Count
        }));
    }

    [HttpPost]
    [Authorize(Roles = "supervisor")]
    public async Task<ActionResult<GroupDto>> Create([FromBody] CreateGroupDto dto)
    {
        var code = dto.Code.Trim().ToUpper();
        if (await db.StudentGroups.AnyAsync(g => g.Code == code))
            return Conflict(new { message = "Código já utilizado." });

        var group = new StudentGroup
        {
            Code = code,
            Name = dto.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim()
        };

        db.StudentGroups.Add(group);
        await db.SaveChangesAsync();
        return Ok(new GroupDto { Id = group.Id, Code = group.Code, Name = group.Name, Description = group.Description, MemberCount = 0 });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "supervisor")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var group = await db.StudentGroups.FindAsync(id);
        if (group == null) return NotFound();
        db.StudentGroups.Remove(group);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ── Schedules ──────────────────────────────────────────────────────────────
    [HttpGet("schedules")]
    public async Task<ActionResult<List<ScheduleDto>>> GetSchedules()
    {
        var schedules = await db.RotationSchedules
            .Include(s => s.Group)
            .Include(s => s.Location)
            .Include(s => s.Preceptor)
            .OrderBy(s => s.StartDate)
            .ToListAsync();

        return Ok(schedules.Select(MapSchedule));
    }

    [HttpPost("schedules")]
    [Authorize(Roles = "supervisor")]
    public async Task<ActionResult<ScheduleDto>> CreateSchedule([FromBody] CreateScheduleDto dto)
    {
        var schedule = new RotationSchedule
        {
            GroupId = dto.GroupId,
            LocationId = dto.LocationId,
            PreceptorId = dto.PreceptorId,
            Shift = dto.Shift,
            PeriodLabel = dto.PeriodLabel.Trim(),
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            ActivityType = dto.ActivityType,
            RequiredHours = dto.RequiredHours > 0 ? dto.RequiredHours : 80,
            Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim()
        };

        db.RotationSchedules.Add(schedule);
        await db.SaveChangesAsync();

        await db.Entry(schedule).Reference(s => s.Group).LoadAsync();
        await db.Entry(schedule).Reference(s => s.Location).LoadAsync();
        if (schedule.PreceptorId.HasValue)
            await db.Entry(schedule).Reference(s => s.Preceptor).LoadAsync();

        return Ok(MapSchedule(schedule));
    }

    [HttpPut("schedules/{id}")]
    [Authorize(Roles = "supervisor")]
    public async Task<ActionResult<ScheduleDto>> UpdateSchedule(Guid id, [FromBody] CreateScheduleDto dto)
    {
        var schedule = await db.RotationSchedules
            .Include(s => s.Group).Include(s => s.Location).Include(s => s.Preceptor)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (schedule == null) return NotFound();

        schedule.GroupId = dto.GroupId;
        schedule.LocationId = dto.LocationId;
        schedule.PreceptorId = dto.PreceptorId;
        schedule.Shift = dto.Shift;
        schedule.PeriodLabel = dto.PeriodLabel.Trim();
        schedule.StartDate = dto.StartDate;
        schedule.EndDate = dto.EndDate;
        schedule.ActivityType = dto.ActivityType;
        schedule.RequiredHours = dto.RequiredHours > 0 ? dto.RequiredHours : 80;
        schedule.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();

        await db.SaveChangesAsync();
        await db.Entry(schedule).Reference(s => s.Group).LoadAsync();
        await db.Entry(schedule).Reference(s => s.Location).LoadAsync();
        if (schedule.PreceptorId.HasValue)
            await db.Entry(schedule).Reference(s => s.Preceptor).LoadAsync();

        return Ok(MapSchedule(schedule));
    }

    [HttpDelete("schedules/{id}")]
    [Authorize(Roles = "supervisor")]
    public async Task<IActionResult> DeleteSchedule(Guid id)
    {
        var schedule = await db.RotationSchedules.FindAsync(id);
        if (schedule == null) return NotFound();
        db.RotationSchedules.Remove(schedule);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static ScheduleDto MapSchedule(RotationSchedule s) => new()
    {
        Id = s.Id, GroupId = s.GroupId, GroupCode = s.Group?.Code ?? string.Empty,
        LocationId = s.LocationId, LocationName = s.Location?.Name ?? string.Empty,
        PreceptorId = s.PreceptorId, PreceptorName = s.Preceptor?.FullName,
        Shift = s.Shift, PeriodLabel = s.PeriodLabel,
        StartDate = s.StartDate, EndDate = s.EndDate,
        ActivityType = s.ActivityType, RequiredHours = s.RequiredHours, Notes = s.Notes
    };
}
