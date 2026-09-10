namespace SKP.OS.Base.Models;

public class LogbookEntry : ISoftDeletable
{
    public int Id { get; set; }
    public DateTimeOffset Date { get; set; }
    public string Entry { get; set; } = string.Empty;
    public bool HasSearchedForJob { get; set; }
    public bool IsDeleted { get; set; }

    public int StudentProfileId { get; set; }
    public StudentProfile StudentProfile { get; set; } = null!;
}