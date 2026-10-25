namespace Gym.Application.Features.Identity.Dtos;

public enum GoogleLoginStatus
{
    Authenticated,
    RegistrationRequired,
    EmailVerificationRequired
}

public sealed record GoogleLoginOutcome(
    GoogleLoginStatus Status,
    TokenResponse? Tokens = null);