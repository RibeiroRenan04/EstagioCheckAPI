using EstagioCheck.API.Data;
using EstagioCheck.API.DTOs;
using EstagioCheck.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EstagioCheck.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FollowupsController(AppDbContext db, IHttpContextAccessor httpContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<FollowupDto>>> GetAll()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")!);
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "";

        var query = db.FormativeFollowups
            .Include(f => f.Student)
            .Include(f => f.Preceptor)
            .Include(f => f.Location)
            .AsQueryable();

        if (role == "aluno")
            // Aluno vê apenas documentos finalizados pelo preceptor ou concluídos
            query = query.Where(f => f.StudentId == userId
                && (f.Status == "finalizado_preceptor" || f.Status == "finalizado_aluno"));
        else if (role == "preceptor")
            query = query.Where(f => f.PreceptorId == userId);
        // supervisor vê todos

        var items = await query.OrderByDescending(f => f.CreatedAt).ToListAsync();
        return Ok(items.Select(Map));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FollowupDto>> Get(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")!);
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "";

        var f = await db.FormativeFollowups
            .Include(x => x.Student).Include(x => x.Preceptor).Include(x => x.Location)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (f == null) return NotFound();

        // Verifica acesso
        if (role == "aluno" && f.StudentId != userId)
            return Forbid();
        if (role == "aluno" && f.Status == "rascunho")
            return Forbid();
        if (role == "preceptor" && f.PreceptorId != userId)
            return Forbid();

        return Ok(Map(f));
    }

    /// <summary>Busca um aluno pelo RGM para preencher o acompanhamento (nome + id).</summary>
    [HttpGet("student-by-rgm/{rgm}")]
    [Authorize(Roles = "preceptor,supervisor")]
    public async Task<ActionResult<StudentLookupDto>> GetStudentByRgm(string rgm)
    {
        var termo = rgm.Trim();
        var student = await db.Users
            .Where(u => u.Role == "aluno" && u.Rgm == termo)
            .Select(u => new StudentLookupDto(u.Id, u.FullName, u.Rgm))
            .FirstOrDefaultAsync();

        return student == null
            ? NotFound(new { message = "Aluno não encontrado para esse RGM." })
            : Ok(student);
    }

    [HttpPost]
    [Authorize(Roles = "preceptor,supervisor")]
    public async Task<ActionResult<FollowupDto>> Create([FromBody] CreateFollowupDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")!);

        var f = new FormativeFollowup
        {
            StudentId = dto.StudentId,
            PreceptorId = userId,
            ScheduleId = dto.ScheduleId,
            GroupId = dto.GroupId,
            LocationId = dto.LocationId,
            Shift = dto.Shift,
            PeriodLabel = dto.PeriodLabel,
            Semester = dto.Semester,
            FollowUpStart = dto.FollowUpStart,
            FollowUpEnd = dto.FollowUpEnd
        };

        db.FormativeFollowups.Add(f);
        await db.SaveChangesAsync();

        await db.Entry(f).Reference(x => x.Student).LoadAsync();
        await db.Entry(f).Reference(x => x.Preceptor).LoadAsync();
        return Ok(Map(f));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<FollowupDto>> Update(Guid id, [FromBody] UpdateFollowupDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")!);
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "";

        var f = await db.FormativeFollowups
            .Include(x => x.Student).Include(x => x.Preceptor).Include(x => x.Location)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (f == null) return NotFound();
        if (f.Status == "finalizado_aluno")
            return Conflict(new { message = "Documento finalizado e assinado por ambas as partes." });
        if (f.Status == "finalizado_preceptor")
            return Conflict(new { message = "Conteúdo bloqueado após finalização do preceptor." });
        if (role == "preceptor" && f.PreceptorId != userId)
            return Forbid();

        f.PosturaPontualidade = dto.PosturaPontualidade;
        f.PosturaEtica = dto.PosturaEtica;
        f.PosturaResponsabilidade = dto.PosturaResponsabilidade;
        f.ComunicacaoEquipe = dto.ComunicacaoEquipe;
        f.ComunicacaoPaciente = dto.ComunicacaoPaciente;
        f.ComunicacaoEscuta = dto.ComunicacaoEscuta;
        f.OrganizacaoPlanejamento = dto.OrganizacaoPlanejamento;
        f.OrganizacaoSeguranca = dto.OrganizacaoSeguranca;
        f.OrganizacaoRegistros = dto.OrganizacaoRegistros;
        f.ParticipacaoIniciativa = dto.ParticipacaoIniciativa;
        f.ParticipacaoAprendizado = dto.ParticipacaoAprendizado;
        f.ParticipacaoAutocritica = dto.ParticipacaoAutocritica;
        f.Potencialidades = dto.Potencialidades;
        f.AspectosAprimorar = dto.AspectosAprimorar;
        f.SituacoesRelevantes = dto.SituacoesRelevantes;
        f.ObservacoesDocente = dto.ObservacoesDocente;
        f.EvolucaoSemanal = dto.EvolucaoSemanal;
        f.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Ok(Map(f));
    }

    [HttpPost("{id}/finalize-preceptor")]
    [Authorize(Roles = "preceptor,supervisor")]
    public async Task<ActionResult<FollowupDto>> FinalizePreceptor(Guid id, [FromBody] FinalizeFollowupDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")!);
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "";

        var f = await db.FormativeFollowups
            .Include(x => x.Student).Include(x => x.Preceptor)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (f == null) return NotFound();
        if (role == "preceptor" && f.PreceptorId != userId) return Forbid();
        if (f.Status != "rascunho")
            return Conflict(new { message = "Já finalizado." });

        var ip = httpContext.HttpContext?.Connection.RemoteIpAddress?.ToString();
        f.Status = "finalizado_preceptor";
        f.PreceptorSignedAt = DateTime.UtcNow;
        f.PreceptorSignedName = dto.SignerName;
        f.PreceptorSignedIp = ip;
        f.PreceptorSignedUserId = userId;
        f.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Ok(Map(f));
    }

    [HttpPost("{id}/finalize-student")]
    [Authorize(Roles = "aluno")]
    public async Task<ActionResult<FollowupDto>> FinalizeStudent(Guid id, [FromBody] FinalizeFollowupDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")!);

        var f = await db.FormativeFollowups
            .Include(x => x.Student).Include(x => x.Preceptor)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (f == null) return NotFound();
        if (f.StudentId != userId) return Forbid();
        if (f.Status != "finalizado_preceptor")
            return Conflict(new { message = "Aguardando finalização do preceptor." });

        var ip = httpContext.HttpContext?.Connection.RemoteIpAddress?.ToString();
        f.Status = "finalizado_aluno";
        f.StudentSignedAt = DateTime.UtcNow;
        f.StudentSignedName = dto.SignerName;
        f.StudentSignedIp = ip;
        f.StudentSignedUserId = userId;
        f.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Ok(Map(f));
    }

    private static FollowupDto Map(FormativeFollowup f) => new()
    {
        Id = f.Id, StudentId = f.StudentId, StudentName = f.Student?.FullName ?? string.Empty,
        PreceptorId = f.PreceptorId, PreceptorName = f.Preceptor?.FullName ?? string.Empty,
        ScheduleId = f.ScheduleId, GroupId = f.GroupId, LocationId = f.LocationId,
        LocationName = f.Location?.Name, Shift = f.Shift, PeriodLabel = f.PeriodLabel,
        Semester = f.Semester, FollowUpStart = f.FollowUpStart, FollowUpEnd = f.FollowUpEnd,
        PosturaPontualidade = f.PosturaPontualidade, PosturaEtica = f.PosturaEtica,
        PosturaResponsabilidade = f.PosturaResponsabilidade,
        ComunicacaoEquipe = f.ComunicacaoEquipe, ComunicacaoPaciente = f.ComunicacaoPaciente,
        ComunicacaoEscuta = f.ComunicacaoEscuta, OrganizacaoPlanejamento = f.OrganizacaoPlanejamento,
        OrganizacaoSeguranca = f.OrganizacaoSeguranca, OrganizacaoRegistros = f.OrganizacaoRegistros,
        ParticipacaoIniciativa = f.ParticipacaoIniciativa, ParticipacaoAprendizado = f.ParticipacaoAprendizado,
        ParticipacaoAutocritica = f.ParticipacaoAutocritica, Potencialidades = f.Potencialidades,
        AspectosAprimorar = f.AspectosAprimorar, SituacoesRelevantes = f.SituacoesRelevantes,
        ObservacoesDocente = f.ObservacoesDocente, EvolucaoSemanal = f.EvolucaoSemanal,
        Status = f.Status, PreceptorSignedAt = f.PreceptorSignedAt, PreceptorSignedName = f.PreceptorSignedName,
        StudentSignedAt = f.StudentSignedAt, StudentSignedName = f.StudentSignedName,
        CreatedAt = f.CreatedAt, UpdatedAt = f.UpdatedAt
    };
}
