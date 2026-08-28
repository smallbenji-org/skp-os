using SKP.OS.Base.Models;

namespace SKP.OS.Backend.Dtos;

public class InfoEntryDto
{
    public InfoEntryDto() { }

    public InfoEntryDto(InfoEntry entry)
    {
        Id = entry.Id;
        Title = entry.Title;
        Content = entry.Content;
        CreatedAt = entry.CreatedAt;
        IsPinned = entry.IsPinned;
        InstructorProfileId = entry.InstructorProfileId;
    }

    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsPinned { get; set; }
    public int InstructorProfileId { get; set; }
}

public class CreateInfoEntryDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
    public int InstructorProfileId { get; set; }
}

public class UpdateInfoEntryDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
    public int InstructorProfileId { get; set; }
}