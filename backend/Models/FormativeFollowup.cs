namespace EstagioCheck.API.Models;

/// <summary>Acompanhamento formativo (ficha qualitativa de desempenho em estágio).</summary>
public class FormativeFollowup
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StudentId { get; set; }
    public Guid PreceptorId { get; set; }
    public Guid? ScheduleId { get; set; }
    public Guid? GroupId { get; set; }
    public Guid? LocationId { get; set; }
    public string? Shift { get; set; }
    public string? PeriodLabel { get; set; }
    public string? Semester { get; set; }
    public DateOnly? FollowUpStart { get; set; }
    public DateOnly? FollowUpEnd { get; set; }

    // ── Dimensões comportamentais (escala de frequência) ──────────────────────
    // Postura profissional e ética
    public string? PosturaPontualidade { get; set; }
    public string? PosturaEtica { get; set; }
    public string? PosturaResponsabilidade { get; set; }

    // Comunicação e trabalho em equipe
    public string? ComunicacaoEquipe { get; set; }
    public string? ComunicacaoPaciente { get; set; }
    public string? ComunicacaoEscuta { get; set; }

    // Organização e segurança no cuidado
    public string? OrganizacaoPlanejamento { get; set; }
    public string? OrganizacaoSeguranca { get; set; }
    public string? OrganizacaoRegistros { get; set; }

    // Participação e desenvolvimento
    public string? ParticipacaoIniciativa { get; set; }
    public string? ParticipacaoAprendizado { get; set; }
    public string? ParticipacaoAutocritica { get; set; }

    // ── Campos descritivos ────────────────────────────────────────────────────
    public string? Potencialidades { get; set; }
    public string? AspectosAprimorar { get; set; }
    public string? SituacoesRelevantes { get; set; }
    public string? ObservacoesDocente { get; set; }
    public string? EvolucaoSemanal { get; set; }

    // ── Status e assinaturas ──────────────────────────────────────────────────
    /// <summary>"rascunho" | "finalizado_preceptor" | "finalizado_aluno"</summary>
    public string Status { get; set; } = "rascunho";

    public DateTime? PreceptorSignedAt { get; set; }
    public string? PreceptorSignedName { get; set; }
    public string? PreceptorSignedIp { get; set; }
    public Guid? PreceptorSignedUserId { get; set; }

    public DateTime? StudentSignedAt { get; set; }
    public string? StudentSignedName { get; set; }
    public string? StudentSignedIp { get; set; }
    public Guid? StudentSignedUserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ApplicationUser Student { get; set; } = null!;
    public ApplicationUser Preceptor { get; set; } = null!;
    public RotationSchedule? Schedule { get; set; }
    public StudentGroup? Group { get; set; }
    public Location? Location { get; set; }
}
