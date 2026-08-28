using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SKP.OS.Backend.Dtos;
using SKP.OS.Base;
using SKP.OS.Base.Models;

namespace SKP.OS.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InstructorProfileController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public InstructorProfileController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    /// <summary>Lists all instructor profiles.</summary>
    /// <remarks>Returns every instructor profile ordered by user name. Requires: authenticated user.</remarks>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var profiles = await _context.InstructorProfiles
            .Include(ip => ip.User)
            .OrderBy(ip => ip.User!.Name)
            .ToListAsync();
        return Ok(profiles.Select(ip => new InstructorProfileDto(ip)));
    }

    /// <summary>Gets a single instructor profile by id.</summary>
    /// <remarks>
    /// Returns the profile including its assigned students.
    /// <para>Returns 404 if the profile does not exist.</para>
    /// </remarks>
    /// <param name="id">The id of the instructor profile.</param>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var profile = await _context.InstructorProfiles
            .Include(ip => ip.User)
            .Include(ip => ip.Students).ThenInclude(s => s.User)
            .FirstOrDefaultAsync(ip => ip.Id == id);
        if (profile == null)
        {
            return NotFound(new { message = "Instructor profile not found." });
        }

        var dto = new InstructorProfileDto(profile)
        {
            StudentProfiles = profile.Students?.Select(s => new StudentProfileDto(s)).ToList() ?? []
        };
        return Ok(dto);
    }

    /// <summary>Gets the instructor profile of the current user.</summary>
    /// <remarks>
    /// Returns the profile for the currently signed-in user, including assigned students.
    /// <para>Returns 404 if the current user has no instructor profile.</para>
    /// </remarks>
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized(new { message = "User not found." });
        }

        var profile = await _context.InstructorProfiles
            .Include(ip => ip.User)
            .Include(ip => ip.Students).ThenInclude(s => s.User)
            .FirstOrDefaultAsync(ip => ip.ApplicationUserId == user.Id);
        if (profile == null)
        {
            return NotFound(new { message = "No instructor profile found for current user." });
        }

        var dto = new InstructorProfileDto(profile)
        {
            StudentProfiles = profile.Students?.Select(s => new StudentProfileDto(s)).ToList() ?? []
        };
        return Ok(dto);
    }

    /// <summary>Creates a new instructor profile.</summary>
    /// <remarks>
    /// Requires the referenced user to exist. Only one profile is allowed per user.
    /// <para>Returns 409 if a profile already exists for the user, 400 if the user does not exist.</para>
    /// </remarks>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInstructorProfileDto dto)
    {
        var userExists = await _userManager.FindByIdAsync(dto.ApplicationUserId);
        if (userExists == null)
        {
            return BadRequest(new { message = "User does not exist." });
        }

        var alreadyExists = await _context.InstructorProfiles
            .AnyAsync(ip => ip.ApplicationUserId == dto.ApplicationUserId);
        if (alreadyExists)
        {
            return Conflict(new { message = "An instructor profile already exists for this user." });
        }

        var profile = new InstructorProfile
        {
            ApplicationUserId = dto.ApplicationUserId,
            Students = []
        };
        _context.InstructorProfiles.Add(profile);
        await _context.SaveChangesAsync();

        return Ok(new InstructorProfileDto(profile));
    }

    /// <summary>Deletes an instructor profile.</summary>
    /// <remarks>Returns 404 if the profile does not exist, otherwise 204 on success.</remarks>
    /// <param name="id">The id of the instructor profile.</param>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var profile = await _context.InstructorProfiles
            .FirstOrDefaultAsync(ip => ip.Id == id);
        if (profile == null)
        {
            return NotFound(new { message = "Instructor profile not found." });
        }

        _context.InstructorProfiles.Remove(profile);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Assigns a student to an instructor.</summary>
    /// <remarks>
    /// Links the given student profile to the instructor profile.
    /// <para>Returns 409 if already assigned, 404 if either profile does not exist.</para>
    /// </remarks>
    /// <param name="id">The id of the instructor profile.</param>
    /// <param name="studentId">The id of the student profile.</param>
    [HttpPost("{id:int}/students/{studentId:int}")]
    public async Task<IActionResult> AddStudent(int id, int studentId)
    {
        var profile = await _context.InstructorProfiles
            .Include(ip => ip.Students)
            .FirstOrDefaultAsync(ip => ip.Id == id);
        if (profile == null)
        {
            return NotFound(new { message = "Instructor profile not found." });
        }

        var student = await _context.StudentProfiles
            .FirstOrDefaultAsync(sp => sp.Id == studentId);
        if (student == null)
        {
            return NotFound(new { message = "Student profile not found." });
        }

        if (profile.Students?.Any(s => s.Id == studentId) == true)
        {
            return Conflict(new { message = "Student is already assigned to this instructor." });
        }

        profile.Students ??= [];
        profile.Students.Add(student);
        await _context.SaveChangesAsync();

        return Ok(new InstructorProfileDto(profile));
    }

    /// <summary>Removes a student from an instructor.</summary>
    /// <remarks>
    /// Unlinks the student profile from the instructor profile.
    /// <para>Returns 404 if the instructor or the assignment does not exist.</para>
    /// </remarks>
    /// <param name="id">The id of the instructor profile.</param>
    /// <param name="studentId">The id of the student profile.</param>
    [HttpDelete("{id:int}/students/{studentId:int}")]
    public async Task<IActionResult> RemoveStudent(int id, int studentId)
    {
        var profile = await _context.InstructorProfiles
            .Include(ip => ip.Students)
            .FirstOrDefaultAsync(ip => ip.Id == id);
        if (profile == null)
        {
            return NotFound(new { message = "Instructor profile not found." });
        }

        var student = profile.Students?.FirstOrDefault(s => s.Id == studentId);
        if (student == null)
        {
            return NotFound(new { message = "Student is not assigned to this instructor." });
        }

        profile.Students!.Remove(student);
        await _context.SaveChangesAsync();

        return Ok(new InstructorProfileDto(profile));
    }
}
