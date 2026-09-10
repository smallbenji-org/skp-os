using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using SKP.OS.Backend.Dtos;
using SKP.OS.Backend.Networking;
using SKP.OS.Base;
using SKP.OS.Base.Models;

namespace SKP.OS.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CheckInController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly Settings _settings;

    public CheckInController(ApplicationDbContext context, Settings settings)
    {
        _context = context;
        _settings = settings;
    }

    /// <summary>Lists check-ins, optionally filtered by student and/or room.</summary>
    /// <remarks>
    /// Filters by <c>studentProfileId</c> and/or <c>roomId</c> when provided.
    /// Check-ins are ordered newest first. Requires: authenticated user.
    /// </remarks>
    /// <param name="studentProfileId">Optional. Filter to a single student profile.</param>
    /// <param name="roomId">Optional. Filter to a single room.</param>
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

    /// <summary>Gets a single check-in by id.</summary>
    /// <remarks>Returns 404 if the check-in does not exist.</remarks>
    /// <param name="id">The id of the check-in.</param>
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

    private IPAddress? GetClientIp()
    {
        var forwarded = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwarded))
        {
            var first = forwarded.Split(',')[0].Trim();
            if (IPAddress.TryParse(first, out var ip))
            {
                return ip;
            }
        }

        return HttpContext.Connection.RemoteIpAddress;
    }

    /// <summary>Creates a new check-in.</summary>
    /// <remarks>
    /// Requires the referenced student profile and room to exist. The student must not be
    /// blocked from checking in.
    /// <para>Returns 400 if the student profile or room does not exist, 403 if the student is blocked from checking in.</para>
    /// </remarks>
    [HttpPost]
    [EnableRateLimiting("checkin")]
    public async Task<IActionResult> Create([FromBody] CreateCheckInDto dto)
    {
        var clientIp = GetClientIp();
        var allowedSubnets = _settings.CheckIn.AllowedSubnets;
        if (allowedSubnets.Length > 0 &&
            (clientIp == null || !SubnetMatcher.IsInAnySubnet(clientIp, allowedSubnets)))
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "Du skal være på skolens Wi-Fi for at tjekke ind." });
        }

        var student = await _context.StudentProfiles
            .FirstOrDefaultAsync(sp => sp.Id == dto.StudentProfileId);
        if (student == null)
        {
            return BadRequest(new { message = "Student profile does not exist." });
        }

        if (student.IsCheckInBlocked)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Student is blocked from checking in." });
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

    /// <summary>Updates an existing check-in.</summary>
    /// <remarks>
    /// <para>Returns 404 if the check-in does not exist, 400 if the room does not exist.</para>
    /// </remarks>
    /// <param name="id">The id of the check-in.</param>
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

    /// <summary>Deletes a check-in.</summary>
    /// <remarks>Returns 404 if the check-in does not exist, otherwise 204 on success.</remarks>
    /// <param name="id">The id of the check-in.</param>
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
