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

    /// <summary>Lists all project templates.</summary>
    /// <remarks>Returns every project template ordered by title. Requires: authenticated user.</remarks>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var templates = await _context.ProjectTemplates
            .OrderBy(pt => pt.Title)
            .ToListAsync();
        return Ok(templates.Select(pt => new ProjectTemplateDto(pt)));
    }

    /// <summary>Gets a single project template by id.</summary>
    /// <remarks>Returns 404 if the template does not exist.</remarks>
    /// <param name="id">The id of the project template.</param>
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

    /// <summary>Creates a new project template.</summary>
    /// <remarks>
    /// Requires the <c>instructorProfileId</c> to reference an existing instructor profile.
    /// <para>Permissions: Instructor only.</para>
    /// <para>Returns 400 if the instructor profile does not exist.</para>
    /// </remarks>
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

    /// <summary>Updates an existing project template.</summary>
    /// <remarks>
    /// <para>Permissions: Instructor only.</para>
    /// <para>Returns 404 if the template does not exist, 400 if the instructor profile does not exist.</para>
    /// </remarks>
    /// <param name="id">The id of the project template.</param>
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

    /// <summary>Deletes a project template.</summary>
    /// <remarks>
    /// <para>Permissions: Instructor only.</para>
    /// <para>Returns 404 if the template does not exist, otherwise 204 on success.</para>
    /// </remarks>
    /// <param name="id">The id of the project template.</param>
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
