using System;
using System.Collections.Generic;
using System.Text;

using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;

using MediatR;

using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Identity.Commands.ForgetPassword
{
    public sealed record SendResetTokenCommand(string Email) : IRequest<Result<Created>>;
    public sealed class SendResetTokenCommandHandler(IIdentityService identityService, ILogger<SendResetTokenCommand> logger, IEmailSender emailSender) : IRequestHandler<SendResetTokenCommand, Result<Created>>
    {
        public async Task<Result<Created>> Handle(SendResetTokenCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation($"Generating Reset Token to Email : {request.Email}");
            var tokenReuslt = await identityService.GenerateResetTokenAsync(request.Email);
            if (tokenReuslt.IsError)
            {
                logger.LogError("Generatig Reset token faild for email {email}", request.Email);
                return tokenReuslt.Errors;
            }

            var clientUrl = "http://localhost:4200";
            var encodedToken = Uri.EscapeDataString(tokenReuslt.Value);
            await emailSender.SendResetPasswordEmailAsync(request.Email, $"{clientUrl}/reset-password?email={request.Email}&token={encodedToken}");
            logger.LogInformation("Reset Token sended to email {email}", request.Email);

            return Result.Created;
        }
    }
}
