using SKP.OS.Base.Models;

namespace SKP.OS.Backend.Dtos;

public class CheckInDto
{
    public CheckInDto() { }

    public CheckInDto(CheckIn checkIn)
    {
        Id = checkIn.Id;
        CheckInTime = checkIn.CheckInTime;
        CheckOutTime = checkIn.CheckOutTime;
        Seat = checkIn.Seat;
        StudentProfileId = checkIn.StudentProfileId;
        RoomId = checkIn.RoomId;
        Room = checkIn.Room != null ? new RoomDto(checkIn.Room) : null;
    }

    public int Id { get; set; }
    public DateTimeOffset CheckInTime { get; set; }
    public DateTimeOffset? CheckOutTime { get; set; }
    public string Seat { get; set; } = string.Empty;
    public int StudentProfileId { get; set; }
    public int RoomId { get; set; }
    public RoomDto? Room { get; set; }
}

public class CreateCheckInDto
{
    public DateTimeOffset CheckInTime { get; set; }
    public DateTimeOffset? CheckOutTime { get; set; }
    public string Seat { get; set; } = string.Empty;
    public int StudentProfileId { get; set; }
    public int RoomId { get; set; }
}

public class UpdateCheckInDto
{
    public DateTimeOffset CheckInTime { get; set; }
    public DateTimeOffset? CheckOutTime { get; set; }
    public string Seat { get; set; } = string.Empty;
    public int RoomId { get; set; }
}