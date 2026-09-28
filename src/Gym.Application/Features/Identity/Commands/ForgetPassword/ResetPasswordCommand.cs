using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;

using MediatR;

using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Identity.Commands.ForgetPassword
{
    public record ResetPasswordCommand(string Email, string ResetToken, string NewPassword) : IRequest<Result<Updated>>;

    public class ResetPasswordCommandHandler(ILogger<ResetPasswordCommand> logger, IIdentityService identityService) : IRequestHandler<ResetPasswordCommand, Result<Updated>>
    {
        public async Task<Result<Updated>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Reseting password for Email {email}", request.Email);
            var ResetResult = await identityService.ResetPasswordAsync(request.Email, request.ResetToken, request.NewPassword);

            if (ResetResult.IsError)
            {
                logger.LogError("Reset passsword faild for email {email} Errors:{errors}", request.Email, ResetResult.TopError);
                return ResetResult.Errors;
            }

            logger.LogInformation("Reset password successfull for user with email : {email}", request.Email);
            return Result.Updated;
        }
    }
}
