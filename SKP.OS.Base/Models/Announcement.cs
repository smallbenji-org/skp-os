namespace SKP.OS.Base.Models;

public class Announcement : ISoftDeletable
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTimeOffset Date { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }
}