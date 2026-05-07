using System.ComponentModel.DataAnnotations;

namespace EstagioCheck.API.DTOs;

public class EvaluationDto
{
    public Guid Id { get; init; }
    public Guid StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public Guid PreceptorId { get; init; }
    public string PreceptorName { get; init; } = string.Empty;
    public short ActivitiesScore { get; init; }
    public short PostureScore { get; init; }
    public short PlanningScore { get; init; }
    public string? Comment { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record CreateEvaluationDto(
    [Required] Guid StudentId,
    Guid? ScheduleId,
    [Required, Range(1, 5)] short ActivitiesScore,
    [Required, Range(1, 5)] short PostureScore,
    [Required, Range(1, 5)] short PlanningScore,
    string? Comment
);
