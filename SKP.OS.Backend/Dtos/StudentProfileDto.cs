using SKP.OS.Base.Models;

namespace SKP.OS.Backend.Dtos;

public class StudentProfileDto
{
    public StudentProfileDto() { }

    public StudentProfileDto(StudentProfile profile)
    {
        Id = profile.Id;
        ApplicationUserId = profile.ApplicationUserId;
        User = profile.User != null ? new UserDto(profile.User) : null;
        StudentType = profile.StudentType;
        ContractType = profile.ContractType;
        IsEuxStudent = profile.IsEuxStudent;
        CompletedHauls = profile.CompletedHauls?.ToList() ?? [];
    }

    public int Id { get; set; }
    public string ApplicationUserId { get; set; } = string.Empty;
    public UserDto? User { get; set; }
    public StudentType StudentType { get; set; }
    public ContractType ContractType { get; set; }
    public bool IsEuxStudent { get; set; }
    public List<ProjectHaul> CompletedHauls { get; set; } = [];
}

public class CreateStudentProfileDto
{
    public string ApplicationUserId { get; set; } = string.Empty;
    public StudentType StudentType { get; set; }
    public ContractType ContractType { get; set; }
    public bool IsEuxStudent { get; set; }
    public List<ProjectHaul> CompletedHauls { get; set; } = [];
}

public class UpdateStudentProfileDto
{
    public StudentType StudentType { get; set; }
    public ContractType ContractType { get; set; }
    public bool IsEuxStudent { get; set; }
    public List<ProjectHaul> CompletedHauls { get; set; } = [];
}