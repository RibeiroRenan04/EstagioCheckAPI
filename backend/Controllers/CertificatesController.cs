using EstagioCheck.API.DTOs;
using EstagioCheck.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EstagioCheck.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CertificatesController(CertificateService certificates) : ControllerBase
{
    /// <summary>Certificado de carga horária do próprio aluno autenticado.</summary>
    [HttpGet("me")]
    public async Task<ActionResult<CertificateDto>> GetMine()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")!);

        var cert = await certificates.ObterAsync(userId);
        return cert == null ? NotFound() : Ok(cert);
    }

    /// <summary>Lista os certificados de todos os alunos (visão do supervisor).</summary>
    [HttpGet]
    [Authorize(Roles = "supervisor")]
    public async Task<ActionResult<List<CertificateDto>>> GetAll()
    {
        return Ok(await certificates.ListarAsync());
    }

    /// <summary>Certificado de carga horária de um aluno específico.</summary>
    [HttpGet("{studentId}")]
    [Authorize(Roles = "supervisor,preceptor")]
    public async Task<ActionResult<CertificateDto>> GetByStudent(Guid studentId)
    {
        var cert = await certificates.ObterAsync(studentId);
        return cert == null ? NotFound() : Ok(cert);
    }
}
