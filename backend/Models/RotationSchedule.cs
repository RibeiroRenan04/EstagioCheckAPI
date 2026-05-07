namespace EstagioCheck.API.Models;

public class RotationSchedule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GroupId { get; set; }
    public Guid LocationId { get; set; }
    public Guid? PreceptorId { get; set; }

    /// <summary>"manha" | "tarde" | "noite"</summary>
    public string Shift { get; set; } = "manha";

    public string PeriodLabel { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    /// <summary>"gestao" | "pic" | "assistencia" | "outro"</summary>
    public string ActivityType { get; set; } = "assistencia";

    public int RequiredHours { get; set; } = 80;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public StudentGroup Group { get; set; } = null!;
    public Location Location { get; set; } = null!;
    public ApplicationUser? Preceptor { get; set; }
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = [];
    public ICollection<FormativeFollowup> Followups { get; set; } = [];
}
