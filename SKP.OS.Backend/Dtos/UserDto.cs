using SKP.OS.Base.Models;

namespace SKP.OS.Backend.Dtos;

public class UserDto
{
    public UserDto() { }

    public UserDto(ApplicationUser user)
    {
        Id = user.Id;
        UserName = user.UserName ?? string.Empty;
        Email = user.Email ?? string.Empty;
        Name = user.Name ?? string.Empty;
    }

    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}