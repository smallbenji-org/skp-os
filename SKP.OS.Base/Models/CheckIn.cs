namespace SKP.OS.Base.Models;

public class CheckIn : ISoftDeletable
{
    public int Id { get; set; }
    public DateTimeOffset CheckInTime { get; set; }
    public DateTimeOffset? CheckOutTime { get; set; }
    public string Seat { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }

    public int StudentProfileId { get; set; }
    public StudentProfile StudentProfile { get; set; } = null!;

    public int RoomId { get; set; }
    public Room Room { get; set; } = null!;
}