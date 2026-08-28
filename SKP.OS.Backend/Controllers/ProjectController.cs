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
public class ProjectController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProjectController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var projects = await _context.Projects
            .Include(p => p.ProjectTemplate)
            .OrderBy(p => p.Title)
            .ToListAsync();
        return Ok(projects.Select(p => new ProjectDto(p)));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var project = await _context.Projects
            .Include(p => p.ProjectTemplate)
            .Include(p => p.Students).ThenInclude(s => s.User)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (project == null)
        {
            return NotFound(new { message = "Project not found." });
        }

        var dto = new ProjectDto(project)
        {
            Students = project.Students?.Select(s => new StudentProfileDto(s)).ToList() ?? []
        };
        return Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectDto dto)
    {
        if (!dto.IsCustomProject && dto.ProjectTemplateId == null)
        {
            return BadRequest(new { message = "ProjectTemplateId is required for non-custom projects." });
        }

        if (dto.ProjectTemplateId != null)
        {
            var templateExists = await _context.ProjectTemplates
                .AnyAsync(pt => pt.Id == dto.ProjectTemplateId);
            if (!templateExists)
            {
                return BadRequest(new { message = "Project template does not exist." });
            }
        }

        var project = new Project
        {
            Title = dto.Title,
            ShortDescription = dto.ShortDescription,
            GitRepoUrl = dto.GitRepoUrl,
            IsCustomProject = dto.IsCustomProject,
            ProjectTemplateId = dto.ProjectTemplateId,
            Students = []
        };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        return Ok(new ProjectDto(project));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProjectDto dto)
    {
        var project = await _context.Projects
            .Include(p => p.ProjectTemplate)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (project == null)
        {
            return NotFound(new { message = "Project not found." });
        }

        if (dto.ProjectTemplateId != null)
        {
            var templateExists = await _context.ProjectTemplates
                .AnyAsync(pt => pt.Id == dto.ProjectTemplateId);
            if (!templateExists)
            {
                return BadRequest(new { message = "Project template does not exist." });
            }
        }

        project.Title = dto.Title;
        project.ShortDescription = dto.ShortDescription;
        project.Evaluation = dto.Evaluation;
        project.Conclusion = dto.Conclusion;
        project.Perspektivering = dto.Perspektivering;
        project.GitRepoUrl = dto.GitRepoUrl;
        project.IsCustomProject = dto.IsCustomProject;
        project.ProjectTemplateId = dto.ProjectTemplateId;
        await _context.SaveChangesAsync();

        return Ok(new ProjectDto(project));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == id);
        if (project == null)
        {
            return NotFound(new { message = "Project not found." });
        }

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/students/{studentId:int}")]
    public async Task<IActionResult> AddStudent(int id, int studentId)
    {
        var project = await _context.Projects
            .Include(p => p.Students)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (project == null)
        {
            return NotFound(new { message = "Project not found." });
        }

        var student = await _context.StudentProfiles
            .FirstOrDefaultAsync(sp => sp.Id == studentId);
        if (student == null)
        {
            return NotFound(new { message = "Student profile not found." });
        }

        if (project.Students?.Any(s => s.Id == studentId) == true)
        {
            return Conflict(new { message = "Student is already assigned to this project." });
        }

        project.Students ??= [];
        project.Students.Add(student);
        await _context.SaveChangesAsync();

        return Ok(new ProjectDto(project));
    }

    [HttpDelete("{id:int}/students/{studentId:int}")]
    public async Task<IActionResult> RemoveStudent(int id, int studentId)
    {
        var project = await _context.Projects
            .Include(p => p.Students)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (project == null)
        {
            return NotFound(new { message = "Project not found." });
        }

        var student = project.Students?.FirstOrDefault(s => s.Id == studentId);
        if (student == null)
        {
            return NotFound(new { message = "Student is not assigned to this project." });
        }

        project.Students!.Remove(student);
        await _context.SaveChangesAsync();

        return Ok(new ProjectDto(project));
    }
}
