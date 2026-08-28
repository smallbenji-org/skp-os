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
public class FFEntryController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public FFEntryController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? studentProfileId = null)
    {
        var query = _context.FFEntries.AsQueryable();
        if (studentProfileId.HasValue)
        {
            query = query.Where(f => f.StudentProfileId == studentProfileId.Value);
        }
        var entries = await query
            .OrderByDescending(f => f.Date)
            .ToListAsync();
        return Ok(entries.Select(f => new FFEntryDto(f)));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var entry = await _context.FFEntries
            .FirstOrDefaultAsync(f => f.Id == id);
        if (entry == null)
        {
            return NotFound(new { message = "FF entry not found." });
        }
        return Ok(new FFEntryDto(entry));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFFEntryDto dto)
    {
        var studentExists = await _context.StudentProfiles
            .AnyAsync(sp => sp.Id == dto.StudentProfileId);
        if (!studentExists)
        {
            return BadRequest(new { message = "Student profile does not exist." });
        }

        var entry = new FFEntry
        {
            Date = dto.Date,
            Duration = dto.Duration,
            Note = dto.Note,
            StudentProfileId = dto.StudentProfileId
        };
        _context.FFEntries.Add(entry);
        await _context.SaveChangesAsync();

        return Ok(new FFEntryDto(entry));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateFFEntryDto dto)
    {
        var entry = await _context.FFEntries
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
        await _context.SaveChangesAsync();

        return Ok(new FFEntryDto(entry));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entry = await _context.FFEntries
            .FirstOrDefaultAsync(f => f.Id == id);
        if (entry == null)
        {
            return NotFound(new { message = "FF entry not found." });
        }

        _context.FFEntries.Remove(entry);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
