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
public class FFEntryController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public FFEntryController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    /// <summary>Lists FVU/FF (grundforløb) entries, optionally filtered by student.</summary>
    /// <remarks>
    /// If <c>studentProfileId</c> is provided, only entries for that student are returned.
    /// Each entry includes the instructor who granted/deducted the hours.
    /// Entries are ordered newest first. Requires: authenticated user.
    /// </remarks>
    /// <param name="studentProfileId">Optional. Filter to a single student profile.</param>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? studentProfileId = null)
    {
        var query = _context.FFEntries
            .Include(f => f.InstructorProfile).ThenInclude(ip => ip.User)
            .AsQueryable();
        if (studentProfileId.HasValue)
        {
            query = query.Where(f => f.StudentProfileId == studentProfileId.Value);
        }
        var entries = await query
            .OrderByDescending(f => f.Date)
            .ToListAsync();
        return Ok(entries.Select(f => new FFEntryDto(f)));
    }

    /// <summary>Gets a single FF entry by id.</summary>
    /// <remarks>Returns 404 if the entry does not exist.</remarks>
    /// <param name="id">The id of the FF entry.</param>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var entry = await _context.FFEntries
            .Include(f => f.InstructorProfile).ThenInclude(ip => ip.User)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (entry == null)
        {
            return NotFound(new { message = "FF entry not found." });
        }
        return Ok(new FFEntryDto(entry));
    }

    /// <summary>Creates a new FF entry.</summary>
    /// <remarks>
    /// Requires the referenced student profile to exist. Grants or deducts FF hours for
    /// the student and records the authenticated instructor as the one who made the change.
    /// <para>Requires: Instructor role.</para>
    /// <para>Returns 400 if the student profile does not exist.</para>
    /// </remarks>
    [HttpPost]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> Create([FromBody] CreateFFEntryDto dto)
    {
        var studentExists = await _context.StudentProfiles
            .AnyAsync(sp => sp.Id == dto.StudentProfileId);
        if (!studentExists)
        {
            return BadRequest(new { message = "Student profile does not exist." });
        }

        int? instructorProfileId = null;
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            var instructorProfile = await _context.InstructorProfiles
                .FirstOrDefaultAsync(ip => ip.ApplicationUserId == user.Id);
            if (instructorProfile == null)
            {
                instructorProfile = new InstructorProfile { ApplicationUserId = user.Id };
                _context.InstructorProfiles.Add(instructorProfile);
                await _context.SaveChangesAsync();
            }
            instructorProfileId = instructorProfile.Id;
        }

        var entry = new FFEntry
        {
            Date = dto.Date,
            Duration = dto.Duration,
            Note = dto.Note,
            StudentProfileId = dto.StudentProfileId,
            InstructorProfileId = instructorProfileId
        };
        _context.FFEntries.Add(entry);
        await _context.SaveChangesAsync();

        var created = await _context.FFEntries
            .Include(f => f.InstructorProfile).ThenInclude(ip => ip.User)
            .FirstAsync(f => f.Id == entry.Id);
        return Ok(new FFEntryDto(created));
    }

    /// <summary>Updates an existing FF entry.</summary>
    /// <remarks>
    /// <para>Requires: Instructor role.</para>
    /// <para>Returns 404 if the entry does not exist, 400 if the student profile does not exist.</para>
    /// </remarks>
    /// <param name="id">The id of the FF entry.</param>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateFFEntryDto dto)
    {
        var entry = await _context.FFEntries
            .Include(f => f.InstructorProfile).ThenInclude(ip => ip.User)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (entry == null)
        {
            return NotFound(new { message = "FF entry not found." });
        }

        var studentExists = await _context.StudentProfiles
            .AnyAsync(sp => sp.Id == dto.StudentProfileId);
        if (!studentExists)
        {
            return BadRequest(new { message = "Student profile does not exist." });
        }

        entry.Date = dto.Date;
        entry.Duration = dto.Duration;
        entry.Note = dto.Note;
        entry.StudentProfileId = dto.StudentProfileId;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { message = "FF-oftelsen er ændret af en anden. Prøv igen." });
        }

        return Ok(new FFEntryDto(entry));
    }

    /// <summary>Deletes an FF entry.</summary>
    /// <remarks>Requires: Instructor role. Returns 404 if the entry does not exist, otherwise 204 on success.</remarks>
    /// <param name="id">The id of the FF entry.</param>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> Delete(int id)
    {
        var entry = await _context.FFEntries
            .FirstOrDefaultAsync(f => f.Id == id);
        if (entry == null)
        {
            return NotFound(new { message = "FF entry not found." });
        }

        entry.IsDeleted = true;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { message = "FF-oftelsen er ændret af en anden. Prøv igen." });
        }
        return NoContent();
    }
}
