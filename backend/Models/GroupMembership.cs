namespace EstagioCheck.API.Models;

public class GroupMembership
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StudentId { get; set; }
    public Guid GroupId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ApplicationUser Student { get; set; } = null!;
    public StudentGroup Group { get; set; } = null!;
}
