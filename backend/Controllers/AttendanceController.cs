using EstagioCheck.API.Data;
using EstagioCheck.API.DTOs;
using EstagioCheck.API.Models;
using EstagioCheck.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EstagioCheck.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceController(AppDbContext db, GeoService geo) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AttendanceRecordDto>>> GetAll([FromQuery] Guid? studentId, [FromQuery] int limit = 200)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")!);
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "aluno";

        var query = db.AttendanceRecords
            .Include(r => r.Student)
            .Include(r => r.Location)
            .Include(r => r.ValidatedBy)
            .AsQueryable();

        if (role == "aluno")
            query = query.Where(r => r.StudentId == userId);
        else if (studentId.HasValue)
            query = query.Where(r => r.StudentId == studentId.Value);

        var recs = await query
            .OrderByDescending(r => r.RecordedAt)
            .Take(limit)
            .ToListAsync();

        return Ok(recs.Select(Map));
    }

    [HttpGet("active-schedule")]
    public async Task<ActionResult<ActiveScheduleDto?>> GetActiveSchedule()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")!);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var currentShift = ShiftFromHour(DateTime.Now.Hour);

        var membership = await db.GroupMemberships.FirstOrDefaultAsync(m => m.StudentId == userId);
        if (membership == null) return Ok(null);

        var schedule = await db.RotationSchedules
            .Include(s => s.Location)
            .Where(s => s.GroupId == membership.GroupId
                     && s.StartDate <= today
                     && s.EndDate >= today
                     && s.Shift == currentShift)
            .FirstOrDefaultAsync();

        // Fallback: qualquer turno hoje
        schedule ??= await db.RotationSchedules
            .Include(s => s.Location)
            .Where(s => s.GroupId == membership.GroupId
                     && s.StartDate <= today
                     && s.EndDate >= today)
            .FirstOrDefaultAsync();

        if (schedule == null) return Ok(null);

        return Ok(new ActiveScheduleDto
        {
            ScheduleId = schedule.Id,
            Shift = schedule.Shift,
            PeriodLabel = schedule.PeriodLabel,
            ActivityType = schedule.ActivityType,
            RequiredHours = schedule.RequiredHours,
            Location = new LocationDto
            {
                Id = schedule.Location.Id,
                Name = schedule.Location.Name,
                Address = schedule.Location.Address,
                Latitude = schedule.Location.Latitude,
                Longitude = schedule.Location.Longitude,
                RadiusMeters = schedule.Location.RadiusMeters,
                IsInstitution = schedule.Location.IsInstitution,
                ShiftStart = schedule.Location.ShiftStart,
                ShiftEnd = schedule.Location.ShiftEnd
            }
        });
    }

    [HttpGet("open-check-in")]
    public async Task<ActionResult> GetOpenCheckIn()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")!);

        var last = await db.AttendanceRecords
            .Where(r => r.StudentId == userId)
            .OrderByDescending(r => r.RecordedAt)
            .Select(r => new { r.Id, r.Type, r.RecordedAt })
            .FirstOrDefaultAsync();

        if (last?.Type == "check_in")
            return Ok(new { id = last.Id, recorded_at = last.RecordedAt });

        return Ok(null);
    }

    [HttpPost]
    public async Task<ActionResult<AttendanceRecordDto>> Create([FromBody] CreateAttendanceDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")!);

        if (dto.Type != "check_in" && dto.Type != "check_out")
            return BadRequest(new { message = "Tipo inválido." });

        // Determina status por geolocalização
        string status = "irregular";
        string? irregularityReason = null;
        double? distanceMeters = null;

        if (dto.LocationId.HasValue)
        {
            var location = await db.Locations.FindAsync(dto.LocationId.Value);
            if (location != null)
            {
                distanceMeters = geo.HaversineMeters(dto.Latitude, dto.Longitude,
                    location.Latitude, location.Longitude);
                if (distanceMeters <= location.RadiusMeters)
                    status = "aprovado";
                else
                    irregularityReason = $"Distância {distanceMeters:0}m excede raio de {location.RadiusMeters}m";
            }
        }
        else
        {
            // Sem local definido → pendente para validação manual
            status = "pendente";
        }

        // Processa foto (base64 → salva localmente ou em blob storage)
        string? photoUrl = null;
        if (!string.IsNullOrEmpty(dto.PhotoBase64))
        {
            // Em produção: enviar para Azure Blob ou S3
            // Para MVP: salva como data URI (pode ser substituído)
            photoUrl = $"data:image/jpeg;base64,{dto.PhotoBase64[..Math.Min(50, dto.PhotoBase64.Length)]}...";
        }

        var record = new AttendanceRecord
        {
            StudentId = userId,
            ScheduleId = dto.ScheduleId,
            LocationId = dto.LocationId,
            Type = dto.Type,
            RecordedAt = DateTime.UtcNow,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            DistanceMeters = distanceMeters,
            PhotoUrl = photoUrl,
            ActivitiesDescription = dto.ActivitiesDescription,
            Status = status,
            IrregularityReason = irregularityReason
        };

        db.AttendanceRecords.Add(record);
        await db.SaveChangesAsync();

        await db.Entry(record).Reference(r => r.Student).LoadAsync();
        if (record.LocationId.HasValue)
            await db.Entry(record).Reference(r => r.Location).LoadAsync();

        return Ok(Map(record));
    }

    [HttpPatch("{id}/validate")]
    [Authorize(Roles = "preceptor,supervisor")]
    public async Task<ActionResult<AttendanceRecordDto>> Validate(Guid id, [FromBody] ValidateAttendanceDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")!);

        var record = await db.AttendanceRecords
            .Include(r => r.Student)
            .Include(r => r.Location)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (record == null) return NotFound();

        record.Status = dto.Approve ? "aprovado" : "irregular";
        if (!dto.Approve && !string.IsNullOrEmpty(dto.Reason))
            record.IrregularityReason = dto.Reason;
        record.ValidatedById = userId;
        record.ValidatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Ok(Map(record));
    }

    private static string ShiftFromHour(int h) =>
        h is >= 6 and < 13 ? "manha" :
        h is >= 13 and < 19 ? "tarde" : "noite";

    private static AttendanceRecordDto Map(AttendanceRecord r) => new()
    {
        Id = r.Id,
        StudentId = r.StudentId,
        StudentName = r.Student?.FullName ?? string.Empty,
        Type = r.Type,
        RecordedAt = r.RecordedAt,
        Latitude = r.Latitude,
        Longitude = r.Longitude,
        DistanceMeters = r.DistanceMeters,
        PhotoUrl = r.PhotoUrl,
        ActivitiesDescription = r.ActivitiesDescription,
        Status = r.Status,
        IrregularityReason = r.IrregularityReason,
        LocationName = r.Location?.Name,
        ScheduleId = r.ScheduleId,
        LocationId = r.LocationId,
        ValidatedByName = r.ValidatedBy?.FullName,
        ValidatedAt = r.ValidatedAt
    };
}
