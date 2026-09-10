using SKP.OS.Base.Models;

namespace SKP.OS.Backend.Dtos;

public class FFEntryDto
{
    public FFEntryDto() { }

    public FFEntryDto(FFEntry entry)
    {
        Id = entry.Id;
        Date = entry.Date;
        Duration = entry.Duration;
        Note = entry.Note;
        StudentProfileId = entry.StudentProfileId;
        InstructorProfileId = entry.InstructorProfileId;
        InstructorName = entry.InstructorProfile?.User?.Name ?? null;
    }

    public int Id { get; set; }
    public DateTimeOffset Date { get; set; }
    public TimeSpan Duration { get; set; }
    public string Note { get; set; } = string.Empty;
    public int StudentProfileId { get; set; }
    public int? InstructorProfileId { get; set; }
    public string? InstructorName { get; set; }
}

public class CreateFFEntryDto
{
    public DateTimeOffset Date { get; set; }
    public TimeSpan Duration { get; set; }
    public string Note { get; set; } = string.Empty;
    public int StudentProfileId { get; set; }
}

public class UpdateFFEntryDto
{
    public DateTimeOffset Date { get; set; }
    public TimeSpan Duration { get; set; }
    public string Note { get; set; } = string.Empty;
    public int StudentProfileId { get; set; }
}