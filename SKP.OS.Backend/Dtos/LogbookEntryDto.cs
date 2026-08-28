using SKP.OS.Base.Models;

namespace SKP.OS.Backend.Dtos;

public class LogbookEntryDto
{
    public LogbookEntryDto() { }

    public LogbookEntryDto(LogbookEntry entry)
    {
        Id = entry.Id;
        Date = entry.Date;
        Entry = entry.Entry;
        HasSearchedForJob = entry.HasSearchedForJob;
        StudentProfileId = entry.StudentProfileId;
    }

    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Entry { get; set; } = string.Empty;
    public bool HasSearchedForJob { get; set; }
    public int StudentProfileId { get; set; }
}

public class CreateLogbookEntryDto
{
    public DateTime Date { get; set; }
    public string Entry { get; set; } = string.Empty;
    public bool HasSearchedForJob { get; set; }
    public int StudentProfileId { get; set; }
}

public class UpdateLogbookEntryDto
{
    public DateTime Date { get; set; }
    public string Entry { get; set; } = string.Empty;
    public bool HasSearchedForJob { get; set; }
    public int StudentProfileId { get; set; }
}