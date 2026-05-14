namespace EstagioCheck.API.Models;

public class Location
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int RadiusMeters { get; set; } = 100;
    public bool IsInstitution { get; set; }

    /// <summary>Horário de início do turno, ex: "07:00"</summary>
    public string ShiftStart { get; set; } = "07:00";

    /// <summary>Horário de fim do turno, ex: "13:00"</summary>
    public string ShiftEnd { get; set; } = "13:00";

    /// <summary>Código CNES quando importado via Busca Saúde DF.</summary>
    public string? CodigoCnes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<RotationSchedule> Schedules { get; set; } = [];
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = [];
}
