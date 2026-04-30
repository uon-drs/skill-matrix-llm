namespace SkillMatrixLlm.Api.Services.EmailServices;

using Contracts;
using Enums;
using Models.Emails;

/// <summary>Sends membership-related notification emails to project managers.</summary>
public class MembershipEmailService(IEmailSender emails)
{
    /// <summary>
    /// Notifies a project manager that a user has accepted or declined a membership invitation.
    /// </summary>
    /// <param name="to">The project manager's email address.</param>
    /// <param name="userDisplayName">Display name of the user who responded.</param>
    /// <param name="projectTitle">Title of the project.</param>
    /// <param name="projectRole">The role the user was invited for.</param>
    /// <param name="response">The user's response — Accepted or Declined.</param>
    public async Task SendMembershipResponseAsync(Models.Emails.EmailAddress to, string userDisplayName, string projectTitle, string projectRole, MembershipStatus response)
        => await emails.SendEmail(to, "Emails/MembershipResponse", new MembershipResponseEmailModel(userDisplayName, projectTitle, projectRole, response));
}
