using SKP.OS.Base.Models;

namespace SKP.OS.Backend.Dtos;

public class ProjectTemplateDto
{
    public ProjectTemplateDto() { }

    public ProjectTemplateDto(ProjectTemplate template)
    {
        Id = template.Id;
        Title = template.Title;
        ShortDescription = template.ShortDescription;
        GitRepoUrl = template.GitRepoUrl;
        Haul = template.Haul;
        StudentType = template.StudentType;
        InstructorProfileId = template.InstructorProfileId;
    }

    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string GitRepoUrl { get; set; } = string.Empty;
    public ProjectHaul Haul { get; set; }
    public StudentType StudentType { get; set; }
    public int InstructorProfileId { get; set; }
}

public class CreateProjectTemplateDto
{
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string GitRepoUrl { get; set; } = string.Empty;
    public ProjectHaul Haul { get; set; }
    public StudentType StudentType { get; set; }
    public int InstructorProfileId { get; set; }
}

public class UpdateProjectTemplateDto
{
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string GitRepoUrl { get; set; } = string.Empty;
    public ProjectHaul Haul { get; set; }
    public StudentType StudentType { get; set; }
    public int InstructorProfileId { get; set; }
}