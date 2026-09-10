using SKP.OS.Base.Models;

namespace SKP.OS.Backend.Dtos;

public class AnnouncementDto
{
    public AnnouncementDto() { }

    public AnnouncementDto(Announcement announcement)
    {
        Id = announcement.Id;
        Title = announcement.Title;
        Message = announcement.Message;
        Date = announcement.Date;
        CreatedAt = announcement.CreatedAt;
        IsActive = announcement.IsActive;
    }

    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class CreateAnnouncementDto
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateAnnouncementDto
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; }
    public bool IsActive { get; set; }
}