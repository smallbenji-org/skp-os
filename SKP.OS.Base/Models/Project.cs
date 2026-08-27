namespace SKP.OS.Base.Models;

public class Project
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string ShortDescription { get; set; }
    public string Evaluation { get; set; }
    public string Conclusion { get; set; }
    public string Perspektivering { get; set; }
    public string GitRepoUrl { get; set; }
    public bool IsCustomProject { get; set; }

    public int? ProjectTemplateId { get; set; }
    public ProjectTemplate ProjectTemplate { get; set; }

    public ICollection<StudentProfile> Students { get; set; }
}
