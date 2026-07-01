namespace Gym.Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendEmailConfirmationAsync(
        string toEmail,
        string confirmationUrl,
        CancellationToken ct = default);
}