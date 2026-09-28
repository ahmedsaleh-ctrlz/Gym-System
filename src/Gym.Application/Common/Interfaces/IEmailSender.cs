namespace Gym.Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendEmailConfirmationAsync(
        string toEmail,
        string confirmationUrl,
        CancellationToken ct = default);

    Task SendResetPasswordEmailAsync(
        string toEmail,
        string ResetUrl,
        CancellationToken ct = default);

    Task SendEmailsToUsersAsync(IEnumerable<string> emails, string subject, string htmlBody, CancellationToken ct = default);
}