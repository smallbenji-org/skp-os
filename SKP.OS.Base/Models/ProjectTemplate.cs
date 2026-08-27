namespace SKP.OS.Base.Models;

public class ProjectTemplate
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string ShortDescription { get; set; }
    public string GitRepoUrl { get; set; }
    public ProjectHaul Haul { get; set; }
    public StudentType StudentType { get; set; }

    public int InstructorProfileId { get; set; }
    public InstructorProfile InstructorProfile { get; set; }

    public ICollection<Project> Projects { get; set; }
}
