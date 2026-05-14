namespace EstagioCheck.API.Models;

/// <summary>Usuário do sistema (aluno, preceptor ou supervisor).</summary>
public class ApplicationUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>"aluno" | "preceptor" | "supervisor"</summary>
    public string Role { get; set; } = "aluno";

    public string? Matricula { get; set; }

    /// <summary>Registro Geral de Matriculado – identificador institucional do aluno.</summary>
    public string? Rgm { get; set; }

    /// <summary>Semestre atual do aluno (7 ou 8).</summary>
    public int? Semester { get; set; }

    /// <summary>Turno: "manha" | "tarde" | "noite"</summary>
    public string? Shift { get; set; }

    public string? Phone { get; set; }
    public string? Institution { get; set; }

    public bool MustChangePassword { get; set; } = false;
    public bool MustSetEmail { get; set; } = false;
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public GroupMembership? GroupMembership { get; set; }
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = [];
    public ICollection<Evaluation> EvaluationsAsStudent { get; set; } = [];
    public ICollection<Evaluation> EvaluationsAsPreceptor { get; set; } = [];
    public ICollection<RotationSchedule> SchedulesAsPreceptor { get; set; } = [];
    public ICollection<FormativeFollowup> FollowupsAsStudent { get; set; } = [];
    public ICollection<FormativeFollowup> FollowupsAsPreceptor { get; set; } = [];
}
