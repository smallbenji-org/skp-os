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
public class AnnouncementController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AnnouncementController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>Lists all announcements, newest first.</summary>
    /// <remarks>Requires: authenticated user.</remarks>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var announcements = await _context.Announcements
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
        return Ok(announcements.Select(a => new AnnouncementDto(a)));
    }

    /// <summary>Gets a single announcement by id.</summary>
    /// <remarks>Returns 404 if the announcement does not exist.</remarks>
    /// <param name="id">The id of the announcement.</param>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var announcement = await _context.Announcements
            .FirstOrDefaultAsync(a => a.Id == id);
        if (announcement == null)
        {
            return NotFound(new { message = "Announcement not found." });
        }
        return Ok(new AnnouncementDto(announcement));
    }

    /// <summary>Creates a new announcement.</summary>
    /// <remarks>
    /// <para>Requires: Instructor role.</para>
    /// <para>Returns 400 if the title is empty.</para>
    /// </remarks>
    [HttpPost]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> Create([FromBody] CreateAnnouncementDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return BadRequest(new { message = "Titel må ikke være tom." });
        }

        var announcement = new Announcement
        {
            Title = dto.Title,
            Message = dto.Message,
            CreatedAt = DateTimeOffset.UtcNow,
            IsActive = dto.IsActive
        };
        _context.Announcements.Add(announcement);
        await _context.SaveChangesAsync();

        return Ok(new AnnouncementDto(announcement));
    }

    /// <summary>Updates an existing announcement.</summary>
    /// <remarks>
    /// <para>Requires: Instructor role.</para>
    /// <para>Returns 404 if the announcement does not exist, 400 if the title is empty.</para>
    /// </remarks>
    /// <param name="id">The id of the announcement.</param>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAnnouncementDto dto)
    {
        var announcement = await _context.Announcements
            .FirstOrDefaultAsync(a => a.Id == id);
        if (announcement == null)
        {
            return NotFound(new { message = "Announcement not found." });
        }

        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return BadRequest(new { message = "Titel må ikke være tom." });
        }

        announcement.Title = dto.Title;
        announcement.Message = dto.Message;
        announcement.IsActive = dto.IsActive;
        await _context.SaveChangesAsync();

        return Ok(new AnnouncementDto(announcement));
    }

    /// <summary>Deletes an announcement.</summary>
    /// <remarks>Requires: Instructor role. Returns 404 if the announcement does not exist, otherwise 204 on success.</remarks>
    /// <param name="id">The id of the announcement.</param>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> Delete(int id)
    {
        var announcement = await _context.Announcements
            .FirstOrDefaultAsync(a => a.Id == id);
        if (announcement == null)
        {
            return NotFound(new { message = "Announcement not found." });
        }

        _context.Announcements.Remove(announcement);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}