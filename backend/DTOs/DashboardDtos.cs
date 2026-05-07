namespace EstagioCheck.API.DTOs;

public class DashboardStatsDto
{
    public int Total { get; init; }
    public int Approved { get; init; }
    public int Irregular { get; init; }
    public int Pending { get; init; }
    public double Hours { get; init; }
    public int Required { get; init; }
    public int PendencyDays { get; init; }
    public double PendencyHours { get; init; }
    public List<PendencyDto> Pendencies { get; init; } = [];
}
