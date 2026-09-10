namespace SKP.OS.Base.Models;

public class StudentProfile : ISoftDeletable
{
    public int Id { get; set; }
    public string ApplicationUserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public StudentType StudentType { get; set; }
    public ContractType ContractType { get; set; }
    public bool IsEuxStudent { get; set; }
    public bool IsCheckInBlocked { get; set; }
    public bool IsDeleted { get; set; }
    public ICollection<ProjectHaul> CompletedHauls { get; set; } = [];
    public ICollection<InstructorProfile> Instructors { get; set; } = [];
    public ICollection<Project> Projects { get; set; } = [];
}