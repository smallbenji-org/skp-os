using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    private readonly UserManager<ApplicationUser> _userManager;

    public ProjectController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    /// <summary>Lists all projects.</summary>
    /// <remarks>Returns every project (with its template and assigned students) ordered by title. Requires: authenticated user.</remarks>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var projects = await _context.Projects
            .Include(p => p.ProjectTemplate)
            .Include(p => p.Students).ThenInclude(s => s.User)
            .OrderBy(p => p.Title)
            .ToListAsync();
        return Ok(projects.Select(p => new ProjectDto(p)
        {
            Students = p.Students?.Select(s => new StudentProfileDto(s)).ToList() ?? []
        }));
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

    /// <summary>Creates a new project from a project template and assigns the current user.</summary>
    /// <remarks>
    /// Copies the template's title, description and repository into a new project linked to the
    /// template, then automatically assigns the current user's student profile to it.
    /// <para>Returns 404 if the template does not exist, 404 if the current user has no student profile.</para>
    /// </remarks>
    /// <param name="templateId">The id of the project template to copy.</param>
    [HttpPost("from-template/{templateId:int}")]
    public async Task<IActionResult> CreateFromTemplate(int templateId)
    {
        var template = await _context.ProjectTemplates
            .FirstOrDefaultAsync(pt => pt.Id == templateId);
        if (template == null)
        {
            return NotFound(new { message = "Project template not found." });
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized(new { message = "User not found." });
        }

        var student = await _context.StudentProfiles
            .Include(sp => sp.User)
            .FirstOrDefaultAsync(sp => sp.ApplicationUserId == user.Id);
        if (student == null)
        {
            return NotFound(new { message = "No student profile found for current user." });
        }

        var project = new Project
        {
            Title = template.Title,
            ShortDescription = template.ShortDescription,
            GitRepoUrl = template.GitRepoUrl,
            IsCustomProject = false,
            ProjectTemplateId = template.Id,
            Students = [student]
        };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var dto = new ProjectDto(project)
        {
            Students = project.Students?.Select(s => new StudentProfileDto(s)).ToList() ?? []
        };
        return Ok(dto);
    }

    /// <summary>Updates an existing project.</summary>
    /// <remarks>
    /// Updates all editable project fields. Passing <c>projectTemplateId</c> requires an existing template.
    /// <para>Only instructors or students assigned to the project may update it.</para>
    /// <para>Returns 404 if the project does not exist, 400 if the template does not exist, 403 for unauthorized users.</para>
    /// </remarks>
    /// <param name="id">The id of the project.</param>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProjectDto dto)
    {
        var project = await _context.Projects
            .Include(p => p.ProjectTemplate)
            .Include(p => p.Students)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (project == null)
        {
            return NotFound(new { message = "Project not found." });
        }

        if (!await CanEditProject(project))
        {
            return Forbid();
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

    /// <summary>Submits a project on behalf of an assigned student.</summary>
    /// <remarks>
    /// Saves the student's project fields and moves the project from <c>Approved</c> to
    /// <c>Submitted</c>.
    /// <para>Only a student assigned to the project may submit it, and only after the instructor has approved it.</para>
    /// <para>Returns 404 if the project does not exist, 403 for unauthorized users, 400 if the project is not approved.</para>
    /// </remarks>
    /// <param name="id">The id of the project.</param>
    [HttpPost("{id:int}/submit")]
    public async Task<IActionResult> Submit(int id, [FromBody] UpdateProjectDto dto)
    {
        var project = await _context.Projects
            .Include(p => p.ProjectTemplate)
            .Include(p => p.Students).ThenInclude(s => s.User)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (project == null)
        {
            return NotFound(new { message = "Project not found." });
        }

        var student = await CurrentUserStudent();
        if (student == null || project.Students?.Any(s => s.Id == student.Id) != true)
        {
            return Forbid();
        }

        if (project.Stage != ProjectStage.Approved)
        {
            return BadRequest(new { message = "Project must be approved before it can be submitted." });
        }

        project.Title = dto.Title;
        project.ShortDescription = dto.ShortDescription;
        project.Evaluation = dto.Evaluation;
        project.Conclusion = dto.Conclusion;
        project.Perspektivering = dto.Perspektivering;
        project.GitRepoUrl = dto.GitRepoUrl;
        project.IsCustomProject = dto.IsCustomProject;
        project.ProjectTemplateId = dto.ProjectTemplateId;
        project.Stage = ProjectStage.Submitted;
        await _context.SaveChangesAsync();

        return Ok(new ProjectDto(project)
        {
            Students = project.Students?.Select(s => new StudentProfileDto(s)).ToList() ?? []
        });
    }

    /// <summary>Changes a project's stage.</summary>
    /// <remarks>
    /// Stages must change step by step: forward only to the immediately following stage
    /// (Created → Approved → Submitted → Evaluated), while going backwards is always allowed.
    /// <para>Returns 404 if the project does not exist, 400 for an invalid transition.</para>
    /// </remarks>
    /// <param name="id">The id of the project.</param>
    [HttpPut("{id:int}/stage")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> UpdateStage(int id, [FromBody] UpdateProjectStageDto dto)
    {
        var project = await _context.Projects
            .Include(p => p.ProjectTemplate)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (project == null)
        {
            return NotFound(new { message = "Project not found." });
        }

        if (!IsValidStageTransition(project.Stage, dto.Stage))
        {
            return BadRequest(new { message = $"Stages must change step by step. Cannot move from {project.Stage} to {dto.Stage}." });
        }

        project.Stage = dto.Stage;
        await _context.SaveChangesAsync();

        return Ok(new ProjectDto(project));
    }

    /// <summary>Updates the instructor feedback for a project.</summary>
    /// <remarks>
    /// Sets the feedback shown to the assigned students. Feedback can only be given after the
    /// project has been submitted.
    /// <para>Only instructors may update feedback.</para>
    /// <para>Returns 404 if the project does not exist, 400 if the project is not yet submitted.</para>
    /// </remarks>
    /// <param name="id">The id of the project.</param>
    [HttpPut("{id:int}/feedback")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> UpdateFeedback(int id, [FromBody] UpdateProjectFeedbackDto dto)
    {
        var project = await _context.Projects
            .Include(p => p.ProjectTemplate)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (project == null)
        {
            return NotFound(new { message = "Project not found." });
        }

        if (project.Stage == ProjectStage.Created || project.Stage == ProjectStage.Approved)
        {
            return BadRequest(new { message = "Feedback can only be given after the project has been submitted." });
        }

        project.Feedback = dto.Feedback;
        await _context.SaveChangesAsync();

        return Ok(new ProjectDto(project));
    }

    private static bool IsValidStageTransition(ProjectStage current, ProjectStage next)
    {
        if (current == next)
        {
            return false;
        }
        return next < current || next == current + 1;
    }

    private async Task<bool> CanEditProject(Project project)
    {
        if (User.IsInRole("Instructor"))
        {
            return true;
        }

        var student = await CurrentUserStudent();
        return student != null && project.Students?.Any(s => s.Id == student.Id) == true;
    }

    private async Task<StudentProfile?> CurrentUserStudent()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return null;
        }

        return await _context.StudentProfiles
            .Include(sp => sp.User)
            .FirstOrDefaultAsync(sp => sp.ApplicationUserId == user.Id);
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

        project.IsDeleted = true;
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
