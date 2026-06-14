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

        var agora = DateTime.UtcNow;

        // Validação inteligente: geolocalização (distância) + janela de horário do turno.
        var location = dto.LocationId.HasValue
            ? await db.Locations.FindAsync(dto.LocationId.Value)
            : null;
        var (status, irregularityReason, distanceMeters) =
            AvaliarRegistro(location, dto.Latitude, dto.Longitude, dto.AccuracyMeters, agora);

        // Foto do registro: guardamos o data URI completo (MVP). Limite de ~5 MB
        // para proteger o banco; o frontend já comprime a imagem antes de enviar.
        const int MaxFotoChars = 7_000_000;
        string? photoUrl = null;
        if (!string.IsNullOrEmpty(dto.PhotoBase64) && dto.PhotoBase64.Length <= MaxFotoChars)
            photoUrl = dto.PhotoBase64;

        var record = new AttendanceRecord
        {
            StudentId = userId,
            ScheduleId = dto.ScheduleId,
            LocationId = dto.LocationId,
            Type = dto.Type,
            RecordedAt = agora,
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

    // Tolerância (minutos) aplicada à janela do turno antes de marcar como pendente.
    private const int ToleranciaTurnoMin = 30;
    // Fuso de Brasília (UTC-3, sem horário de verão) para comparar o horário do turno.
    private const int OffsetBrasiliaHoras = -3;

    /// <summary>
    /// Validação inteligente do registro: combina distância (geofence, ajustada pela precisão do GPS),
    /// janela de horário do turno e a regra de sexta-feira (registro na instituição de ensino).
    /// - Sem local definido → "pendente" (validação manual).
    /// - Fora do raio OU sexta-feira fora da instituição → "irregular".
    /// - Dentro do raio, porém fora do horário do turno → "pendente".
    /// - Dentro do raio e dentro do horário → "aprovado".
    /// </summary>
    private (string status, string? reason, double? distance) AvaliarRegistro(
        Location? location, double lat, double lon, double? accuracyMeters, DateTime recordedAtUtc)
    {
        if (location == null)
            return ("pendente", "Sem local vinculado. Aguardando validação manual.", null);

        var distance = geo.HaversineMeters(lat, lon, location.Latitude, location.Longitude);
        var motivos = new List<string>();

        // Distância efetiva considera a margem de erro do GPS (mais tolerante).
        var precisao = accuracyMeters.GetValueOrDefault(0);
        var distanciaEfetiva = Math.Max(0, distance - precisao);
        var foraDoRaio = distanciaEfetiva > location.RadiusMeters;
        if (foraDoRaio)
            motivos.Add($"Fora do raio ({distance:0}m, precisão GPS ±{precisao:0}m; limite {location.RadiusMeters}m)");

        var localDateTime = recordedAtUtc.AddHours(OffsetBrasiliaHoras);
        var horaLocal = localDateTime.TimeOfDay;
        var foraDoTurno = false;
        if (TimeSpan.TryParse(location.ShiftStart, out var inicio) &&
            TimeSpan.TryParse(location.ShiftEnd, out var fim))
        {
            var tol = TimeSpan.FromMinutes(ToleranciaTurnoMin);
            if (horaLocal < inicio - tol || horaLocal > fim + tol)
            {
                foraDoTurno = true;
                motivos.Add($"Registro às {horaLocal:hh\\:mm} fora do turno ({location.ShiftStart}–{location.ShiftEnd})");
            }
        }

        // Regra de sexta-feira: o registro deve ser feito na instituição de ensino.
        var sextaForaInstituicao = localDateTime.DayOfWeek == DayOfWeek.Friday && !location.IsInstitution;
        if (sextaForaInstituicao)
            motivos.Add("Sexta-feira: o registro deve ser feito na instituição de ensino");

        if (foraDoRaio || sextaForaInstituicao)
            return ("irregular", string.Join("; ", motivos), distance);
        if (foraDoTurno)
            return ("pendente", string.Join("; ", motivos), distance);
        return ("aprovado", null, distance);
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
