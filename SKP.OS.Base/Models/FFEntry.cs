namespace SKP.OS.Base.Models;

public class FFEntry : ISoftDeletable
{
    public int Id { get; set; }

    public DateTimeOffset Date { get; set; }

    public TimeSpan Duration { get; set; }

    public string Note { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }

    public int StudentProfileId { get; set; }
    public StudentProfile StudentProfile { get; set; } = null!;

    public int? InstructorProfileId { get; set; }
    public InstructorProfile? InstructorProfile { get; set; }
}