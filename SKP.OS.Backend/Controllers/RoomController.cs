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
public class RoomController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public RoomController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>Lists all rooms.</summary>
    /// <remarks>Returns every room ordered by name. Requires: authenticated user.</remarks>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var rooms = await _context.Rooms
            .OrderBy(r => r.Name)
            .ToListAsync();
        return Ok(rooms.Select(r => new RoomDto(r)));
    }

    /// <summary>Gets a single room by id.</summary>
    /// <remarks>Returns 404 if the room does not exist.</remarks>
    /// <param name="id">The id of the room.</param>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var room = await _context.Rooms
            .FirstOrDefaultAsync(r => r.Id == id);
        if (room == null)
        {
            return NotFound(new { message = "Room not found." });
        }
        return Ok(new RoomDto(room));
    }

    /// <summary>Creates a new room.</summary>
    /// <remarks>Permissions: Instructor only.</remarks>
    [HttpPost]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> Create([FromBody] CreateRoomDto dto)
    {
        var room = new Room
        {
            Name = dto.Name,
            Location = dto.Location
        };
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        return Ok(new RoomDto(room));
    }

    /// <summary>Updates an existing room.</summary>
    /// <remarks>
    /// <para>Permissions: Instructor only.</para>
    /// <para>Returns 404 if the room does not exist.</para>
    /// </remarks>
    /// <param name="id">The id of the room.</param>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoomDto dto)
    {
        var room = await _context.Rooms
            .FirstOrDefaultAsync(r => r.Id == id);
        if (room == null)
        {
            return NotFound(new { message = "Room not found." });
        }

        room.Name = dto.Name;
        room.Location = dto.Location;
        await _context.SaveChangesAsync();

        return Ok(new RoomDto(room));
    }

    /// <summary>Deletes a room.</summary>
    /// <remarks>
    /// <para>Permissions: Instructor only.</para>
    /// <para>Returns 404 if the room does not exist, otherwise 204 on success.</para>
    /// </remarks>
    /// <param name="id">The id of the room.</param>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> Delete(int id)
    {
        var room = await _context.Rooms
            .FirstOrDefaultAsync(r => r.Id == id);
        if (room == null)
        {
            return NotFound(new { message = "Room not found." });
        }

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
