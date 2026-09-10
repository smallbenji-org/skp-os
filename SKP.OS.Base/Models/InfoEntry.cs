using System.ComponentModel.DataAnnotations;

namespace SKP.OS.Base.Models;

public class InfoEntry : ISoftDeletable
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsPinned { get; set; }
    public bool IsDeleted { get; set; }

    public int InstructorProfileId { get; set; }
    public InstructorProfile InstructorProfile { get; set; }
}