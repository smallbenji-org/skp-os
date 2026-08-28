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
    }

    public int Id { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan Duration { get; set; }
    public string Note { get; set; } = string.Empty;
    public int StudentProfileId { get; set; }
}

public class CreateFFEntryDto
{
    public DateTime Date { get; set; }
    public TimeSpan Duration { get; set; }
    public string Note { get; set; } = string.Empty;
    public int StudentProfileId { get; set; }
}

public class UpdateFFEntryDto
{
    public DateTime Date { get; set; }
    public TimeSpan Duration { get; set; }
    public string Note { get; set; } = string.Empty;
    public int StudentProfileId { get; set; }
}