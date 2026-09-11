namespace SKP.OS.Base.Models;

public class Project : ISoftDeletable
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string Evaluation { get; set; } = string.Empty;
    public string Conclusion { get; set; } = string.Empty;
    public string Perspektivering { get; set; } = string.Empty;
    public string GitRepoUrl { get; set; } = string.Empty;
    public bool IsCustomProject { get; set; }
    public ProjectStage Stage { get; set; } = ProjectStage.Created;
    public string? Feedback { get; set; }
    public bool IsDeleted { get; set; }

    public int? ProjectTemplateId { get; set; }
    public ProjectTemplate ProjectTemplate { get; set; } = null!;

    public ICollection<StudentProfile> Students { get; set; } = [];
}