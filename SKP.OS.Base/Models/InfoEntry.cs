using System.ComponentModel.DataAnnotations;

namespace SKP.OS.Base.Models;

public class InfoEntry
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsPinned { get; set; }

    public int InstructorProfileId { get; set; }
    public InstructorProfile InstructorProfile { get; set; }
}
