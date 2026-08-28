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
public class StudentProfileController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public StudentProfileController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var profiles = await _context.StudentProfiles
            .Include(sp => sp.User)
            .OrderBy(sp => sp.User!.Name)
            .ToListAsync();
        return Ok(profiles.Select(sp => new StudentProfileDto(sp)));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var profile = await _context.StudentProfiles
            .Include(sp => sp.User)
            .Include(sp => sp.Instructors).ThenInclude(i => i.User)
            .Include(sp => sp.Projects)
            .FirstOrDefaultAsync(sp => sp.Id == id);
        if (profile == null)
        {
            return NotFound(new { message = "Student profile not found." });
        }
        return Ok(new StudentProfileDto(profile));
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized(new { message = "User not found." });
        }

        var profile = await _context.StudentProfiles
            .Include(sp => sp.User)
            .Include(sp => sp.Instructors).ThenInclude(i => i.User)
            .FirstOrDefaultAsync(sp => sp.ApplicationUserId == user.Id);
        if (profile == null)
        {
            return NotFound(new { message = "No student profile found for current user." });
        }
        return Ok(new StudentProfileDto(profile));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStudentProfileDto dto)
    {
        var userExists = await _userManager.FindByIdAsync(dto.ApplicationUserId);
        if (userExists == null)
        {
            return BadRequest(new { message = "User does not exist." });
        }

        var alreadyExists = await _context.StudentProfiles
            .AnyAsync(sp => sp.ApplicationUserId == dto.ApplicationUserId);
        if (alreadyExists)
        {
            return Conflict(new { message = "A student profile already exists for this user." });
        }

        var profile = new StudentProfile
        {
            ApplicationUserId = dto.ApplicationUserId,
            StudentType = dto.StudentType,
            ContractType = dto.ContractType,
            IsEuxStudent = dto.IsEuxStudent,
            CompletedHauls = dto.CompletedHauls ?? [],
            Instructors = [],
            Projects = []
        };
        _context.StudentProfiles.Add(profile);
        await _context.SaveChangesAsync();

        return Ok(new StudentProfileDto(profile));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStudentProfileDto dto)
    {
        var profile = await _context.StudentProfiles
            .Include(sp => sp.User)
            .FirstOrDefaultAsync(sp => sp.Id == id);
        if (profile == null)
        {
            return NotFound(new { message = "Student profile not found." });
        }

        profile.StudentType = dto.StudentType;
        profile.ContractType = dto.ContractType;
        profile.IsEuxStudent = dto.IsEuxStudent;
        profile.CompletedHauls = dto.CompletedHauls ?? [];
        await _context.SaveChangesAsync();

        return Ok(new StudentProfileDto(profile));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var profile = await _context.StudentProfiles
            .FirstOrDefaultAsync(sp => sp.Id == id);
        if (profile == null)
        {
            return NotFound(new { message = "Student profile not found." });
        }

        _context.StudentProfiles.Remove(profile);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/instructors/{instructorId:int}")]
    public async Task<IActionResult> AddInstructor(int id, int instructorId)
    {
        var profile = await _context.StudentProfiles
            .Include(sp => sp.Instructors)
            .FirstOrDefaultAsync(sp => sp.Id == id);
        if (profile == null)
        {
            return NotFound(new { message = "Student profile not found." });
        }

        var instructor = await _context.InstructorProfiles
            .FirstOrDefaultAsync(ip => ip.Id == instructorId);
        if (instructor == null)
        {
            return NotFound(new { message = "Instructor profile not found." });
        }

        if (profile.Instructors?.Any(i => i.Id == instructorId) == true)
        {
            return Conflict(new { message = "Instructor is already assigned to this student." });
        }

        profile.Instructors ??= [];
        profile.Instructors.Add(instructor);
        await _context.SaveChangesAsync();

        return Ok(new StudentProfileDto(profile));
    }

    [HttpDelete("{id:int}/instructors/{instructorId:int}")]
    public async Task<IActionResult> RemoveInstructor(int id, int instructorId)
    {
        var profile = await _context.StudentProfiles
            .Include(sp => sp.Instructors)
            .FirstOrDefaultAsync(sp => sp.Id == id);
        if (profile == null)
        {
            return NotFound(new { message = "Student profile not found." });
        }

        var instructor = profile.Instructors?.FirstOrDefault(i => i.Id == instructorId);
        if (instructor == null)
        {
            return NotFound(new { message = "Instructor is not assigned to this student." });
        }

        profile.Instructors!.Remove(instructor);
        await _context.SaveChangesAsync();

        return Ok(new StudentProfileDto(profile));
    }
}
