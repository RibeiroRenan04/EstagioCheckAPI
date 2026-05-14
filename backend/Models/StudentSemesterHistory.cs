namespace EstagioCheck.API.Models;

/// <summary>Histórico de semestre do aluno com carga horária acumulada.</summary>
public class StudentSemesterHistory
{
    public int Id { get; set; }
    public Guid StudentId { get; set; }
    public ApplicationUser Student { get; set; } = null!;
    public int Semester { get; set; }
    public decimal TotalHours { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}
