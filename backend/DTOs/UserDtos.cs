namespace EstagioCheck.API.DTOs;

public class UserDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Matricula { get; init; }
    public string Role { get; init; } = string.Empty;
    public Guid? GroupId { get; init; }
    public string? GroupCode { get; init; }
    public string? GroupName { get; init; }
}

public record AssignGroupDto(Guid? GroupId);
