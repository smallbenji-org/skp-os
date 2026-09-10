namespace SKP.OS.Base.Models;

public class Room : ISoftDeletable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }

    public ICollection<CheckIn> CheckIns { get; set; } = [];
}