using SKP.OS.Base.Models;

namespace SKP.OS.Backend.Dtos;

public class RoomDto
{
    public RoomDto() { }

    public RoomDto(Room room)
    {
        Id = room.Id;
        Name = room.Name;
        Location = room.Location;
    }

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}

public class CreateRoomDto
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}

public class UpdateRoomDto
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}