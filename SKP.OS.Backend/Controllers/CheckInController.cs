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
public class CheckInController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CheckInController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? studentProfileId = null, [FromQuery] int? roomId = null)
    {
        var query = _context.CheckIns
            .Include(c => c.Room)
            .AsQueryable();
        if (studentProfileId.HasValue)
        {
            query = query.Where(c => c.StudentProfileId == studentProfileId.Value);
        }
        if (roomId.HasValue)
        {
            query = query.Where(c => c.RoomId == roomId.Value);
        }
        var checkIns = await query
            .OrderByDescending(c => c.CheckInTime)
            .ToListAsync();
        return Ok(checkIns.Select(c => new CheckInDto(c)));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var checkIn = await _context.CheckIns
            .Include(c => c.Room)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (checkIn == null)
        {
            return NotFound(new { message = "Check-in not found." });
        }
        return Ok(new CheckInDto(checkIn));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCheckInDto dto)
    {
        var studentExists = await _context.StudentProfiles
            .AnyAsync(sp => sp.Id == dto.StudentProfileId);
        if (!studentExists)
        {
            return BadRequest(new { message = "Student profile does not exist." });
        }

        var roomExists = await _context.Rooms
            .AnyAsync(r => r.Id == dto.RoomId);
        if (!roomExists)
        {
            return BadRequest(new { message = "Room does not exist." });
        }

        var checkIn = new CheckIn
        {
            CheckInTime = dto.CheckInTime,
            CheckOutTime = dto.CheckOutTime,
            Seat = dto.Seat,
            StudentProfileId = dto.StudentProfileId,
            RoomId = dto.RoomId
        };
        _context.CheckIns.Add(checkIn);
        await _context.SaveChangesAsync();

        var created = await _context.CheckIns
            .Include(c => c.Room)
            .FirstAsync(c => c.Id == checkIn.Id);
        return Ok(new CheckInDto(created));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCheckInDto dto)
    {
        var checkIn = await _context.CheckIns
            .Include(c => c.Room)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (checkIn == null)
        {
            return NotFound(new { message = "Check-in not found." });
        }

        var roomExists = await _context.Rooms
            .AnyAsync(r => r.Id == dto.RoomId);
        if (!roomExists)
        {
            return BadRequest(new { message = "Room does not exist." });
        }

        checkIn.CheckInTime = dto.CheckInTime;
        checkIn.CheckOutTime = dto.CheckOutTime;
        checkIn.Seat = dto.Seat;
        checkIn.RoomId = dto.RoomId;
        await _context.SaveChangesAsync();

        return Ok(new CheckInDto(checkIn));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var checkIn = await _context.CheckIns
            .FirstOrDefaultAsync(c => c.Id == id);
        if (checkIn == null)
        {
            return NotFound(new { message = "Check-in not found." });
        }

        _context.CheckIns.Remove(checkIn);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
