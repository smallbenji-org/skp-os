namespace SKP.OS.Base.Models;

public class InstructorProfile : ISoftDeletable
{
    public int Id { get; set; }
    public string ApplicationUserId { get; set; }
    public ApplicationUser User { get; set; }
    public bool IsDeleted { get; set; }
    public ICollection<StudentProfile> Students { get; set; }
}