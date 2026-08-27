namespace SKP.OS.Base.Models;

public class LogbookEntry
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Entry { get; set; }
    public bool HasSearchedForJob { get; set; }

    public int StudentProfileId { get; set; }
    public StudentProfile StudentProfile { get; set; }
}
