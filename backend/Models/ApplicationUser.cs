namespace EstagioCheck.API.Models;

/// <summary>Usuário do sistema (aluno, preceptor ou supervisor).</summary>
public class ApplicationUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>"aluno" | "preceptor" | "supervisor"</summary>
    public string Role { get; set; } = "aluno";

    public string? Matricula { get; set; }
    public string? Phone { get; set; }
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
