using System.ComponentModel.DataAnnotations;

namespace EstagioCheck.API.DTOs;

public record CreateAttendanceDto(
    [Required] double Latitude,
    [Required] double Longitude,
    [Required] string Type, // "check_in" | "check_out"
    Guid? ScheduleId,
    Guid? LocationId,
    string? ActivitiesDescription,
    string? PhotoBase64
);

public record ValidateAttendanceDto(
    [Required] bool Approve,
    string? Reason
);

public class AttendanceRecordDto
{
    public Guid Id { get; init; }
    public Guid StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public DateTime RecordedAt { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public double? DistanceMeters { get; init; }
    public string? PhotoUrl { get; init; }
    public string? ActivitiesDescription { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? IrregularityReason { get; init; }
    public string? LocationName { get; init; }
    public Guid? ScheduleId { get; init; }
    public Guid? LocationId { get; init; }
    public string? ValidatedByName { get; init; }
    public DateTime? ValidatedAt { get; init; }
}

public class PendencyDto
{
    public DateOnly PendencyDate { get; init; }
    public Guid? ScheduleId { get; init; }
    public string LocationName { get; init; } = string.Empty;
    public double ExpectedHours { get; init; }
}

public class ActiveScheduleDto
{
    public Guid ScheduleId { get; init; }
    public string Shift { get; init; } = string.Empty;
    public string PeriodLabel { get; init; } = string.Empty;
    public string ActivityType { get; init; } = string.Empty;
    public int RequiredHours { get; init; }
    public LocationDto Location { get; init; } = null!;
}
