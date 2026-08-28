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

    /// <summary>Lists all projects.</summary>
    /// <remarks>Returns every project (with its template) ordered by title. Requires: authenticated user.</remarks>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var projects = await _context.Projects
            .Include(p => p.ProjectTemplate)
            .OrderBy(p => p.Title)
            .ToListAsync();
        return Ok(projects.Select(p => new ProjectDto(p)));
    }

    /// <summary>Gets a single project by id.</summary>
    /// <remarks>
    /// Returns the project including its template and assigned students.
    /// <para>Returns 404 if the project does not exist.</para>
    /// </remarks>
    /// <param name="id">The id of the project.</param>
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

    /// <summary>Creates a new project.</summary>
    /// <remarks>
    /// <c>projectTemplateId</c> is required for non-custom projects and must reference an
    /// existing project template.
    /// <para>Returns 400 if the project template is missing or does not exist.</para>
    /// </remarks>
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

    /// <summary>Updates an existing project.</summary>
    /// <remarks>
    /// Updates all editable project fields. Passing <c>projectTemplateId</c> requires an existing template.
    /// <para>Returns 404 if the project does not exist, 400 if the template does not exist.</para>
    /// </remarks>
    /// <param name="id">The id of the project.</param>
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

    /// <summary>Deletes a project.</summary>
    /// <remarks>Returns 404 if the project does not exist, otherwise 204 on success.</remarks>
    /// <param name="id">The id of the project.</param>
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

    /// <summary>Assigns a student to a project.</summary>
    /// <remarks>
    /// Links the given student profile to the project.
    /// <para>Returns 409 if already assigned, 404 if the project or student does not exist.</para>
    /// </remarks>
    /// <param name="id">The id of the project.</param>
    /// <param name="studentId">The id of the student profile.</param>
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

    /// <summary>Removes a student from a project.</summary>
    /// <remarks>
    /// Unlinks the student profile from the project.
    /// <para>Returns 404 if the project or the assignment does not exist.</para>
    /// </remarks>
    /// <param name="id">The id of the project.</param>
    /// <param name="studentId">The id of the student profile.</param>
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
