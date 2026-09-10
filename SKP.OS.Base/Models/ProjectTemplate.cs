namespace SKP.OS.Base.Models;

public class ProjectTemplate : ISoftDeletable
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string GitRepoUrl { get; set; } = string.Empty;
    public ProjectHaul Haul { get; set; }
    public StudentType StudentType { get; set; }
    public bool IsDeleted { get; set; }

    public int InstructorProfileId { get; set; }
    public InstructorProfile InstructorProfile { get; set; } = null!;

    public ICollection<Project> Projects { get; set; } = [];
}