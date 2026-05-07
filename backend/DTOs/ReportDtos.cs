namespace EstagioCheck.API.DTOs;

public class ReportRowDto
{
    public Guid StudentId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public int Required { get; init; }
    public double Hours { get; init; }
    public int Approved { get; init; }
    public int Irregular { get; init; }
    public int PendencyDays { get; init; }
    public double PendencyHours { get; init; }
    public double ProgressPercent { get; init; }
    public bool CertificateReleased { get; init; }
}
