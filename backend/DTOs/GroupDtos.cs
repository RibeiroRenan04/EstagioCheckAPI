using System.ComponentModel.DataAnnotations;

namespace EstagioCheck.API.DTOs;

public class GroupDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int MemberCount { get; init; }
}

public record CreateGroupDto(
    [Required, MaxLength(20)] string Code,
    [Required, MaxLength(200)] string Name,
    string? Description
);

public class ScheduleDto
{
    public Guid Id { get; init; }
    public Guid GroupId { get; init; }
    public string GroupCode { get; init; } = string.Empty;
    public Guid LocationId { get; init; }
    public string LocationName { get; init; } = string.Empty;
    public Guid? PreceptorId { get; init; }
    public string? PreceptorName { get; init; }
    public string Shift { get; init; } = string.Empty;
    public string PeriodLabel { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public string ActivityType { get; init; } = string.Empty;
    public int RequiredHours { get; init; }
    public string? Notes { get; init; }
}

public record CreateScheduleDto(
    [Required] Guid GroupId,
    [Required] Guid LocationId,
    Guid? PreceptorId,
    [Required] string Shift,
    [Required, MaxLength(100)] string PeriodLabel,
    [Required] DateOnly StartDate,
    [Required] DateOnly EndDate,
    [Required] string ActivityType,
    int RequiredHours,
    string? Notes
);
