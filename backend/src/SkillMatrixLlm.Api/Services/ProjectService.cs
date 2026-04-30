namespace SkillMatrixLlm.Api.Services;

using Data;
using Enums;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Projects;
using Models.User;
using ProjectEntity = Data.Entities.Project;

/// <summary>Manages the full lifecycle of projects.</summary>
public class ProjectService(AppDbContext db)
{
    private static readonly Dictionary<ProjectStatus, ProjectStatus[]> ValidTransitions = new()
    {
        [ProjectStatus.Draft] = [ProjectStatus.Open],
        [ProjectStatus.Open] = [ProjectStatus.TeamConfirmed, ProjectStatus.Closed],
        [ProjectStatus.TeamConfirmed] = [ProjectStatus.Closed],
        [ProjectStatus.Closed] = [],
    };

    /// <summary>
    /// Creates a new project in Draft status.
    /// </summary>
    /// <param name="createdByUserId">Application user ID of the project manager creating the project.</param>
    /// <param name="request">Project details.</param>
    /// <returns>The created project.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the creating user does not exist.</exception>
    public async Task<Project> CreateAsync(Guid createdByUserId, CreateProjectRequest request)
    {
        var user = await db.Users.FindAsync(createdByUserId)
            ?? throw new KeyNotFoundException($"User {createdByUserId} not found.");

        var entity = new ProjectEntity
        {
            Title = request.Title,
            Description = request.Description,
            DesiredTeamSize = request.DesiredTeamSize,
            Timeline = request.Timeline,
            Status = ProjectStatus.Draft,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow,
        };

        db.Projects.Add(entity);
        await db.SaveChangesAsync();

        return ToDto(entity, new UserDto(user.Id, user.DisplayName, user.Email, user.Role));
    }

    /// <summary>
    /// Updates an existing project's details. Only permitted when the project is in Draft or Open status.
    /// </summary>
    /// <param name="projectId">Project ID.</param>
    /// <param name="request">Updated project details.</param>
    /// <returns>The updated project.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the project does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the project status does not allow edits.</exception>
    public async Task<Project> UpdateAsync(Guid projectId, UpdateProjectRequest request)
    {
        var project = await db.Projects.Include(p => p.CreatedByUser).FirstOrDefaultAsync(p => p.Id == projectId)
            ?? throw new KeyNotFoundException($"Project {projectId} not found.");

        if (project.Status is not (ProjectStatus.Draft or ProjectStatus.Open))
            throw new InvalidOperationException($"Project details can only be edited when status is Draft or Open (current: {project.Status}).");

        project.Title = request.Title;
        project.Description = request.Description;
        project.DesiredTeamSize = request.DesiredTeamSize;
        project.Timeline = request.Timeline;

        await db.SaveChangesAsync();

        return ToDto(project, ToUserDto(project.CreatedByUser!));
    }

    /// <summary>
    /// Transitions a project to a new lifecycle status.
    /// </summary>
    /// <param name="projectId">Project ID.</param>
    /// <param name="newStatus">The target status.</param>
    /// <returns>The updated project.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the project does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the transition is not valid from the current status.</exception>
    public async Task<Project> TransitionStatusAsync(Guid projectId, ProjectStatus newStatus)
    {
        var project = await db.Projects.Include(p => p.CreatedByUser).FirstOrDefaultAsync(p => p.Id == projectId)
            ?? throw new KeyNotFoundException($"Project {projectId} not found.");

        if (!ValidTransitions[project.Status].Contains(newStatus))
            throw new InvalidOperationException($"Cannot transition project from {project.Status} to {newStatus}.");

        project.Status = newStatus;
        await db.SaveChangesAsync();

        return ToDto(project, ToUserDto(project.CreatedByUser!));
    }

    /// <summary>
    /// Lists projects with an optional status filter, ordered by creation date descending.
    /// </summary>
    /// <param name="status">Optional status filter.</param>
    /// <returns>Matching projects.</returns>
    public async Task<List<Project>> ListAsync(ProjectStatus? status)
    {
        var query = db.Projects.Include(p => p.CreatedByUser).AsQueryable();

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => ToDto(p, ToUserDto(p.CreatedByUser!)))
            .ToListAsync();
    }

    /// <summary>
    /// Returns full project detail including associated teams (with memberships) and recommendations.
    /// </summary>
    /// <param name="projectId">Project ID.</param>
    /// <returns>Project detail.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the project does not exist.</exception>
    public async Task<ProjectDetailDto> GetAsync(Guid projectId)
    {
        var project = await db.Projects
            .Include(p => p.CreatedByUser)
            .FirstOrDefaultAsync(p => p.Id == projectId)
            ?? throw new KeyNotFoundException($"Project {projectId} not found.");

        var teams = await db.Teams
            .Where(t => t.ProjectId == projectId)
            .Select(t => new TeamDto(
                t.Id,
                t.Source,
                t.Status,
                t.CreatedAt,
                db.TeamMemberships
                    .Where(tm => tm.TeamId == t.Id)
                    .Include(tm => tm.User)
                    .Select(tm => new TeamMembership(
                        tm.Id,
                        new UserDto(tm.User!.Id, tm.User.DisplayName, tm.User.Email, tm.User.Role),
                        tm.ProjectRole,
                        tm.MembershipStatus))
                    .ToList()
            ))
            .ToListAsync();

        var recommendations = await db.Recommendations
            .Where(r => r.ProjectId == projectId)
            .OrderBy(r => r.CreatedAt)
            .Select(r => new Recommendation(r.Id, r.ProjectId, r.TeamId, r.RawResponse, r.CreatedAt))
            .ToListAsync();

        return new ProjectDetailDto(
            project.Id,
            project.Title,
            project.Description,
            project.DesiredTeamSize,
            project.Timeline,
            project.Status,
            ToUserDto(project.CreatedByUser!),
            project.CreatedAt,
            teams,
            recommendations
        );
    }

    private static Project ToDto(ProjectEntity entity, UserDto createdByUser) =>
        new(entity.Id, entity.Title, entity.Description, entity.DesiredTeamSize,
            entity.Timeline, entity.Status, createdByUser, entity.CreatedAt);

    private static UserDto ToUserDto(Data.Entities.User user) =>
        new(user.Id, user.DisplayName, user.Email, user.Role);
}
