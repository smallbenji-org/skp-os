using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SKP.OS.Backend.Dtos;
using SKP.OS.Base;
using SKP.OS.Base.Models;

namespace SKP.OS.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LogbookEntryController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public LogbookEntryController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>Lists logbook entries, optionally filtered by student.</summary>
    /// <remarks>
    /// If <c>studentProfileId</c> is provided, only entries for that student are returned.
    /// Entries are ordered newest first. Requires: authenticated user.
    /// </remarks>
    /// <param name="studentProfileId">Optional. Filter to a single student profile.</param>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? studentProfileId = null)
    {
        var query = _context.LogbookEntries.AsQueryable();
        if (studentProfileId.HasValue)
        {
            query = query.Where(l => l.StudentProfileId == studentProfileId.Value);
        }
        var entries = await query
            .OrderByDescending(l => l.Date)
            .ToListAsync();
        return Ok(entries.Select(l => new LogbookEntryDto(l)));
    }

    /// <summary>Gets a single logbook entry by id.</summary>
    /// <remarks>Returns 404 if the entry does not exist.</remarks>
    /// <param name="id">The id of the logbook entry.</param>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var entry = await _context.LogbookEntries
            .FirstOrDefaultAsync(l => l.Id == id);
        if (entry == null)
        {
            return NotFound(new { message = "Logbook entry not found." });
        }
        return Ok(new LogbookEntryDto(entry));
    }

    /// <summary>Creates a new logbook entry.</summary>
    /// <remarks>
    /// Requires the referenced student profile to exist.
    /// <para>Returns 400 if the student profile does not exist.</para>
    /// </remarks>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLogbookEntryDto dto)
    {
        var studentExists = await _context.StudentProfiles
            .AnyAsync(sp => sp.Id == dto.StudentProfileId);
        if (!studentExists)
        {
            return BadRequest(new { message = "Student profile does not exist." });
        }

        var entry = new LogbookEntry
        {
            Date = dto.Date,
            Entry = dto.Entry,
            HasSearchedForJob = dto.HasSearchedForJob,
            StudentProfileId = dto.StudentProfileId
        };
        _context.LogbookEntries.Add(entry);
        await _context.SaveChangesAsync();

        return Ok(new LogbookEntryDto(entry));
    }

    /// <summary>Updates an existing logbook entry.</summary>
    /// <remarks>
    /// <para>Returns 404 if the entry does not exist, 400 if the student profile does not exist.</para>
    /// </remarks>
    /// <param name="id">The id of the logbook entry.</param>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateLogbookEntryDto dto)
    {
        var entry = await _context.LogbookEntries
            .FirstOrDefaultAsync(l => l.Id == id);
        if (entry == null)
        {
            return NotFound(new { message = "Logbook entry not found." });
        }

        var studentExists = await _context.StudentProfiles
            .AnyAsync(sp => sp.Id == dto.StudentProfileId);
        if (!studentExists)
        {
            return BadRequest(new { message = "Student profile does not exist." });
        }

        entry.Date = dto.Date;
        entry.Entry = dto.Entry;
        entry.HasSearchedForJob = dto.HasSearchedForJob;
        entry.StudentProfileId = dto.StudentProfileId;
        await _context.SaveChangesAsync();

        return Ok(new LogbookEntryDto(entry));
    }

    /// <summary>Deletes a logbook entry.</summary>
    /// <remarks>Returns 404 if the entry does not exist, otherwise 204 on success.</remarks>
    /// <param name="id">The id of the logbook entry.</param>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entry = await _context.LogbookEntries
            .FirstOrDefaultAsync(l => l.Id == id);
        if (entry == null)
        {
            return NotFound(new { message = "Logbook entry not found." });
        }

        _context.LogbookEntries.Remove(entry);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
