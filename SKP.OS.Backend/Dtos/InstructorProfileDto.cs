using SKP.OS.Base.Models;

namespace SKP.OS.Backend.Dtos;

public class InstructorProfileDto
{
    public InstructorProfileDto() { }

    public InstructorProfileDto(InstructorProfile profile)
    {
        Id = profile.Id;
        ApplicationUserId = profile.ApplicationUserId;
        User = profile.User != null ? new UserDto(profile.User) : null;
    }

    public int Id { get; set; }
    public string ApplicationUserId { get; set; } = string.Empty;
    public UserDto? User { get; set; }
    public List<StudentProfileDto> StudentProfiles { get; set; } = [];
}

public class CreateInstructorProfileDto
{
    public string ApplicationUserId { get; set; } = string.Empty;
}

public class UpdateInstructorProfileDto
{
    public string ApplicationUserId { get; set; } = string.Empty;
}