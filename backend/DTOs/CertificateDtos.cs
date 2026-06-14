namespace EstagioCheck.API.DTOs;

/// <summary>Dados do certificado de carga horária de estágio de um aluno.</summary>
public class CertificateDto
{
    public Guid StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public string? Rgm { get; init; }
    public string? GroupName { get; init; }

    /// <summary>Horas efetivamente cumpridas (registros aprovados).</summary>
    public double CompletedHours { get; init; }

    /// <summary>Carga horária exigida (soma dos rodízios do grupo).</summary>
    public int RequiredHours { get; init; }

    public double ProgressPercent { get; init; }

    /// <summary>Verdadeiro quando o aluno atingiu a carga horária exigida.</summary>
    public bool Eligible { get; init; }

    public string? PeriodLabel { get; init; }
    public List<string> Locations { get; init; } = [];
    public string? Institution { get; init; }

    public DateTime IssuedAt { get; init; }

    /// <summary>Código determinístico para conferência do certificado.</summary>
    public string VerificationCode { get; init; } = string.Empty;
}
