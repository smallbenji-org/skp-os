namespace SKP.OS.Base.Models;

public class StudentProfile
{
    public int Id { get; set; }
    public string ApplicationUserId { get; set; }
    public ApplicationUser User { get; set; }
    public StudentType StudentType { get; set; }
    public ContractType ContractType { get; set; }
    public bool IsEuxStudent { get; set; }
    public ICollection<ProjectHaul> CompletedHauls { get; set; }
    public ICollection<InstructorProfile> Instructors { get; set; }
    public ICollection<Project> Projects { get; set; }
}
