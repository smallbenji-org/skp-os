namespace SKP.OS.Base.Models;

public class FFEntry
{
    public int Id { get; set; }

    public DateTimeOffset Date { get; set; }

    public TimeSpan Duration { get; set; }

    public string Note { get; set; }

    public int StudentProfileId { get; set; }
    public StudentProfile StudentProfile { get; set; }
}
