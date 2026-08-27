namespace SKP.OS.Base.Models;

public class Room
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Location { get; set; }

    public ICollection<CheckIn> CheckIns { get; set; }
}
