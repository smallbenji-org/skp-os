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
public class InfoEntryController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public InfoEntryController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>Lists info entries, optionally filtered to pinned only.</summary>
    /// <remarks>
    /// If <c>pinned</c> is provided, only entries matching that pinned state are returned.
    /// Entries are ordered pinned-first, then newest first. Requires: authenticated user.
    /// </remarks>
    /// <param name="pinned">Optional. Filter by pinned state.</param>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? pinned = null)
    {
        var query = _context.InfoEntries.AsQueryable();
        if (pinned.HasValue)
        {
            query = query.Where(i => i.IsPinned == pinned.Value);
        }
        var entries = await query
            .OrderByDescending(i => i.IsPinned)
            .ThenByDescending(i => i.CreatedAt)
            .ToListAsync();
        return Ok(entries.Select(i => new InfoEntryDto(i)));
    }

    /// <summary>Gets a single info entry by id.</summary>
    /// <remarks>Returns 404 if the entry does not exist.</remarks>
    /// <param name="id">The id of the info entry.</param>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var entry = await _context.InfoEntries
            .FirstOrDefaultAsync(i => i.Id == id);
        if (entry == null)
        {
            return NotFound(new { message = "Info entry not found." });
        }
        return Ok(new InfoEntryDto(entry));
    }

    /// <summary>Creates a new info entry.</summary>
    /// <remarks>
    /// Requires the referenced instructor profile to exist. The created timestamp is set automatically.
    /// <para>Returns 400 if the instructor profile does not exist.</para>
    /// </remarks>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInfoEntryDto dto)
    {
        var instructorExists = await _context.InstructorProfiles
            .AnyAsync(ip => ip.Id == dto.InstructorProfileId);
        if (!instructorExists)
        {
            return BadRequest(new { message = "Instructor profile does not exist." });
        }

        var entry = new InfoEntry
        {
            Title = dto.Title,
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow,
            IsPinned = dto.IsPinned,
            InstructorProfileId = dto.InstructorProfileId
        };
        _context.InfoEntries.Add(entry);
        await _context.SaveChangesAsync();

        return Ok(new InfoEntryDto(entry));
    }

    /// <summary>Updates an existing info entry.</summary>
    /// <remarks>
    /// <para>Returns 404 if the entry does not exist, 400 if the instructor profile does not exist.</para>
    /// </remarks>
    /// <param name="id">The id of the info entry.</param>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateInfoEntryDto dto)
    {
        var entry = await _context.InfoEntries
            .FirstOrDefaultAsync(i => i.Id == id);
        if (entry == null)
        {
            return NotFound(new { message = "Info entry not found." });
        }

        var instructorExists = await _context.InstructorProfiles
            .AnyAsync(ip => ip.Id == dto.InstructorProfileId);
        if (!instructorExists)
        {
            return BadRequest(new { message = "Instructor profile does not exist." });
        }

        entry.Title = dto.Title;
        entry.Content = dto.Content;
        entry.IsPinned = dto.IsPinned;
        entry.InstructorProfileId = dto.InstructorProfileId;
        await _context.SaveChangesAsync();

        return Ok(new InfoEntryDto(entry));
    }

    /// <summary>Deletes an info entry.</summary>
    /// <remarks>Returns 404 if the entry does not exist, otherwise 204 on success.</remarks>
    /// <param name="id">The id of the info entry.</param>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entry = await _context.InfoEntries
            .FirstOrDefaultAsync(i => i.Id == id);
        if (entry == null)
        {
            return NotFound(new { message = "Info entry not found." });
        }

        _context.InfoEntries.Remove(entry);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
