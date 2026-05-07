using System.ComponentModel.DataAnnotations;

namespace EstagioCheck.API.DTOs;

public class FollowupDto
{
    public Guid Id { get; init; }
    public Guid StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public Guid PreceptorId { get; init; }
    public string PreceptorName { get; init; } = string.Empty;
    public Guid? ScheduleId { get; init; }
    public Guid? GroupId { get; init; }
    public Guid? LocationId { get; init; }
    public string? LocationName { get; init; }
    public string? Shift { get; init; }
    public string? PeriodLabel { get; init; }
    public string? Semester { get; init; }
    public DateOnly? FollowUpStart { get; init; }
    public DateOnly? FollowUpEnd { get; init; }

    // Dimensões
    public string? PosturaPontualidade { get; init; }
    public string? PosturaEtica { get; init; }
    public string? PosturaResponsabilidade { get; init; }
    public string? ComunicacaoEquipe { get; init; }
    public string? ComunicacaoPaciente { get; init; }
    public string? ComunicacaoEscuta { get; init; }
    public string? OrganizacaoPlanejamento { get; init; }
    public string? OrganizacaoSeguranca { get; init; }
    public string? OrganizacaoRegistros { get; init; }
    public string? ParticipacaoIniciativa { get; init; }
    public string? ParticipacaoAprendizado { get; init; }
    public string? ParticipacaoAutocritica { get; init; }

    // Descritivos
    public string? Potencialidades { get; init; }
    public string? AspectosAprimorar { get; init; }
    public string? SituacoesRelevantes { get; init; }
    public string? ObservacoesDocente { get; init; }
    public string? EvolucaoSemanal { get; init; }

    // Status / assinaturas
    public string Status { get; init; } = string.Empty;
    public DateTime? PreceptorSignedAt { get; init; }
    public string? PreceptorSignedName { get; init; }
    public DateTime? StudentSignedAt { get; init; }
    public string? StudentSignedName { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record CreateFollowupDto(
    [Required] Guid StudentId,
    Guid? ScheduleId,
    Guid? GroupId,
    Guid? LocationId,
    string? Shift,
    string? PeriodLabel,
    string? Semester,
    DateOnly? FollowUpStart,
    DateOnly? FollowUpEnd
);

public class UpdateFollowupDto
{
    public string? PosturaPontualidade { get; init; }
    public string? PosturaEtica { get; init; }
    public string? PosturaResponsabilidade { get; init; }
    public string? ComunicacaoEquipe { get; init; }
    public string? ComunicacaoPaciente { get; init; }
    public string? ComunicacaoEscuta { get; init; }
    public string? OrganizacaoPlanejamento { get; init; }
    public string? OrganizacaoSeguranca { get; init; }
    public string? OrganizacaoRegistros { get; init; }
    public string? ParticipacaoIniciativa { get; init; }
    public string? ParticipacaoAprendizado { get; init; }
    public string? ParticipacaoAutocritica { get; init; }
    public string? Potencialidades { get; init; }
    public string? AspectosAprimorar { get; init; }
    public string? SituacoesRelevantes { get; init; }
    public string? ObservacoesDocente { get; init; }
    public string? EvolucaoSemanal { get; init; }
}

public record FinalizeFollowupDto(
    [Required] string SignerName
);
