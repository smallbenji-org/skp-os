using Microsoft.AspNetCore.Identity;

namespace SKP.OS.Base.Models;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; }
}
