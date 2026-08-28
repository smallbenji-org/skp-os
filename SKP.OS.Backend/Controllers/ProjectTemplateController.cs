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
public class ProjectTemplateController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProjectTemplateController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var templates = await _context.ProjectTemplates
            .OrderBy(pt => pt.Title)
            .ToListAsync();
        return Ok(templates.Select(pt => new ProjectTemplateDto(pt)));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var template = await _context.ProjectTemplates
            .FirstOrDefaultAsync(pt => pt.Id == id);
        if (template == null)
        {
            return NotFound(new { message = "Project template not found." });
        }
        return Ok(new ProjectTemplateDto(template));
    }

    [HttpPost]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> Create([FromBody] CreateProjectTemplateDto dto)
    {
        var instructorExists = await _context.InstructorProfiles
            .AnyAsync(ip => ip.Id == dto.InstructorProfileId);
        if (!instructorExists)
        {
            return BadRequest(new { message = "Instructor profile does not exist." });
        }

        var template = new ProjectTemplate
        {
            Title = dto.Title,
            ShortDescription = dto.ShortDescription,
            GitRepoUrl = dto.GitRepoUrl,
            Haul = dto.Haul,
            StudentType = dto.StudentType,
            InstructorProfileId = dto.InstructorProfileId
        };
        _context.ProjectTemplates.Add(template);
        await _context.SaveChangesAsync();

        return Ok(new ProjectTemplateDto(template));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProjectTemplateDto dto)
    {
        var template = await _context.ProjectTemplates
            .FirstOrDefaultAsync(pt => pt.Id == id);
        if (template == null)
        {
            return NotFound(new { message = "Project template not found." });
        }

        var instructorExists = await _context.InstructorProfiles
            .AnyAsync(ip => ip.Id == dto.InstructorProfileId);
        if (!instructorExists)
        {
            return BadRequest(new { message = "Instructor profile does not exist." });
        }

        template.Title = dto.Title;
        template.ShortDescription = dto.ShortDescription;
        template.GitRepoUrl = dto.GitRepoUrl;
        template.Haul = dto.Haul;
        template.StudentType = dto.StudentType;
        template.InstructorProfileId = dto.InstructorProfileId;
        await _context.SaveChangesAsync();

        return Ok(new ProjectTemplateDto(template));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> Delete(int id)
    {
        var template = await _context.ProjectTemplates
            .FirstOrDefaultAsync(pt => pt.Id == id);
        if (template == null)
        {
            return NotFound(new { message = "Project template not found." });
        }

        _context.ProjectTemplates.Remove(template);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
