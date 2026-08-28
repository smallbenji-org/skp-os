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
