using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SKP.OS.Backend.Dtos;
using SKP.OS.Base.Models;
using SKP.OS.Base;

namespace SKP.OS.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;

    private static readonly string[] AllowedRoles = { "Student", "Instructor" };

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _context = context;
    }

    /// <summary>Registers a new user account.</summary>
    /// <remarks>
    /// Creates an <see cref="ApplicationUser"/> and assigns it the requested role
    /// ("Student" by default, or "Instructor"). A matching student/instructor profile is
    /// automatically created for the new user.
    /// <para>Requires: anonymous.</para>
    /// <para>Returns 200 with the created user, or 400 if validation fails.</para>
    /// </remarks>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var role = string.IsNullOrWhiteSpace(dto.Role) ? "Student" : dto.Role;
        if (!AllowedRoles.Contains(role))
        {
            return BadRequest(new { message = $"Invalid role. Must be one of: {string.Join(", ", AllowedRoles)}." });
        }

        var user = new ApplicationUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            Name = dto.Name
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = string.Join("; ", result.Errors.Select(e => e.Description)) });
        }

        if (!await _roleManager.RoleExistsAsync(role))
        {
            await _roleManager.CreateAsync(new IdentityRole(role));
        }
        await _userManager.AddToRoleAsync(user, role);

        if (role == "Student")
        {
            _context.StudentProfiles.Add(new StudentProfile { ApplicationUserId = user.Id });
        }
        else if (role == "Instructor")
        {
            _context.InstructorProfiles.Add(new InstructorProfile { ApplicationUserId = user.Id });
        }
        await _context.SaveChangesAsync();

        return Ok(new UserDto(user));
    }

    /// <summary>Signs an existing user in with username/email and password.</summary>
    /// <remarks>
    /// Authenticates the user via cookie-based sign-in. Accepts either a username or an email
    /// in the <c>userName</c> field.
    /// <para>Requires: anonymous.</para>
    /// <para>Returns 200 with the user, or 401 if credentials are invalid.</para>
    /// </remarks>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.UserName)
            ?? await _userManager.FindByEmailAsync(dto.UserName);

        if (user == null)
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        var result = await _signInManager.PasswordSignInAsync(
            user, dto.Password, isPersistent: true, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new UserDto(user));
    }

    /// <summary>Signs the current user out.</summary>
    /// <remarks>Requires: authenticated user.</remarks>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok();
    }

    /// <summary>Returns basic information about the current authenticated user.</summary>
    /// <remarks>
    /// Returns the name and email of the currently signed-in user.
    /// <para>Requires: authenticated user.</para>
    /// <para>Returns 200 with the user's name/email, or 401 if not authenticated.</para>
    /// </remarks>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized(new { message = "User not found." });
        }

        return Ok(new MeDto { Name = user.Name ?? string.Empty, Email = user.Email ?? string.Empty });
    }
}
