using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;

using MediatR;

using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Identity.Commands.ResendConfirmationEmail;

public sealed class ResendConfirmationCommandHandler(ILogger<ResendConfirmationCommand> logger, IIdentityService identityService, IEmailSender emailSender) : IRequestHandler<ResendConfirmationCommand, Result<Created>>
{
    public async Task<Result<Created>> Handle(ResendConfirmationCommand request, CancellationToken cancellationToken)
    {
        var generatedTokenResult = await identityService.GenerateEmailConfirmationUrlByEmailAsync(request.Email);
        if(generatedTokenResult.IsError)
        {
            logger.LogError("Failed to generate email confirmation token for {Email}. Errors: {Errors}", request.Email, generatedTokenResult.Errors);
            return generatedTokenResult.Errors;
        }

        await emailSender.SendEmailConfirmationAsync(request.Email, generatedTokenResult.Value, cancellationToken);

        return Result.Created;
    }
}