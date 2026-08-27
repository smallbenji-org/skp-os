namespace SKP.OS.Base.Models;

public class CheckIn
{
    public int Id { get; set; }
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string Seat { get; set; }

    public int StudentProfileId { get; set; }
    public StudentProfile StudentProfile { get; set; }

    public int RoomId { get; set; }
    public Room Room { get; set; }
}
