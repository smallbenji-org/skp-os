using SKP.OS.Base.Models;

namespace SKP.OS.Backend.Dtos;

public class ProjectDto
{
    public ProjectDto() { }

    public ProjectDto(Project project)
    {
        Id = project.Id;
        Title = project.Title;
        ShortDescription = project.ShortDescription;
        Evaluation = project.Evaluation;
        Conclusion = project.Conclusion;
        Perspektivering = project.Perspektivering;
        GitRepoUrl = project.GitRepoUrl;
        IsCustomProject = project.IsCustomProject;
        ProjectTemplateId = project.ProjectTemplateId;
        ProjectTemplate = project.ProjectTemplate != null ? new ProjectTemplateDto(project.ProjectTemplate) : null;
    }

    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string Evaluation { get; set; } = string.Empty;
    public string Conclusion { get; set; } = string.Empty;
    public string Perspektivering { get; set; } = string.Empty;
    public string GitRepoUrl { get; set; } = string.Empty;
    public bool IsCustomProject { get; set; }
    public int? ProjectTemplateId { get; set; }
    public ProjectTemplateDto? ProjectTemplate { get; set; }
    public List<StudentProfileDto> Students { get; set; } = [];
}

public class CreateProjectDto
{
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string GitRepoUrl { get; set; } = string.Empty;
    public bool IsCustomProject { get; set; }
    public int? ProjectTemplateId { get; set; }
}

public class UpdateProjectDto
{
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string Evaluation { get; set; } = string.Empty;
    public string Conclusion { get; set; } = string.Empty;
    public string Perspektivering { get; set; } = string.Empty;
    public string GitRepoUrl { get; set; } = string.Empty;
    public bool IsCustomProject { get; set; }
    public int? ProjectTemplateId { get; set; }
}