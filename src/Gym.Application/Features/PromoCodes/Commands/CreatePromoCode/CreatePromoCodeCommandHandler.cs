using Gym.Application.Common.Interfaces;
using Gym.Application.Common.Interfaces.BackgroundJobsServices;
using Gym.Domain.Common.Result;
using Gym.Domain.Identity;
using Gym.Domain.Notifications.Enums;
using Gym.Domain.PromoCodes;
using Gym.Domain.PromoCodes.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.PromoCodes.Commands.CreatePromoCodeCommand
{
    public sealed class CreatePromoCodeCommandHandler(
    IAppDbContext context,
    HybridCache cache,
    INotificationBackgroundJobs notificationBackgroundJobs, ILogger<CreatePromoCodeCommandHandler> logger, IEmailBackgroundJobs emailBackgroundJobs, IIdentityService identityService)
    : IRequestHandler<CreatePromoCodeCommand, Result<Created>>
    {
        public async Task<Result<Created>> Handle(
            CreatePromoCodeCommand request,
            CancellationToken ct)
        {
            var result = PromoCode.Create(
                request.Code,
                request.PlanId,
                request.DiscountType,
                request.DiscountValue,
                request.MaxDiscount,
                request.MinimumPurchaseAmount,
                request.Audience,
                request.UsageLimit,
                request.ExpiresAtUtc);

            if (result.IsError)
            {
                logger.LogError("Cannot create a promo code due to : {Errors}", result.Errors);
                return result.TopError;
            }

            context.PromoCodes.Add(result.Value);
            logger.LogInformation("Promo Code {Code} Added Successfully", request.Code);
            await context.SaveChangesAsync(ct);

            await cache.RemoveByTagAsync("PromoCodes", ct);

            IEnumerable<int> personIds;
            IEnumerable<string> userIds;

            if (request.Audience == PromoCodeAudience.Everyone)
            {
                var userIdsResult = await identityService
                .GetUsersIdsByRoleAsync(
                    Role.Member,
                    ct);

                userIds = userIdsResult.Value;

                personIds = await context.Members.AsNoTracking().Select(m => m.PersonId).ToListAsync();
            }
            else
            {
                personIds = await context.Subscriptions
                    .Select(s => s.Member.PersonId)
                    .Distinct()
                    .ToListAsync(ct);

                var subscribedPersonIds = personIds;

                var newMemberPersonIds = await context.Members
                    .Where(m => !subscribedPersonIds.Contains(m.PersonId))
                    .Select(m => m.PersonId)
                    .ToListAsync(ct);

                var userIdsResult = await identityService.GetUsersIdsByPersonIdsAsync(
                    newMemberPersonIds,
                    ct);
                userIds = userIdsResult.Value;
            }

            var htmlBody = $$"""
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>New Promo Code Available</title>
</head>

<body style="margin:0;padding:0;background:#f3f6fb;font-family:Segoe UI,Arial,sans-serif;">

    <table role="presentation"
           width="100%"
           cellspacing="0"
           cellpadding="0"
           style="padding:40px 0;">

        <tr>
            <td align="center">

                <table role="presentation"
                       width="600"
                       cellspacing="0"
                       cellpadding="0"
                       style="
                           width:600px;
                           max-width:100%;
                           background:#ffffff;
                           border-radius:18px;
                           overflow:hidden;
                           box-shadow:0 10px 30px rgba(0,0,0,.08);">

                    <!-- Header -->
                    <tr>
                        <td style="
                            background:linear-gradient(135deg,#2563eb,#1d4ed8);
                            padding:40px 30px;
                            text-align:center;">

                            <div style="font-size:48px;line-height:1;">
                                🎉
                            </div>

                            <h1 style="
                                color:#ffffff;
                                margin:18px 0 0 0;
                                font-size:28px;
                                font-weight:700;">
                                New Promo Code Available
                            </h1>

                            <p style="
                                color:#dbeafe;
                                margin:10px 0 0 0;
                                font-size:16px;">
                                We've got something special for you!
                            </p>

                        </td>
                    </tr>

                    <!-- Content -->
                    <tr>
                        <td style="padding:40px 35px;">

                            <h2 style="
                                margin:0 0 15px 0;
                                color:#111827;
                                font-size:22px;">
                                A new offer is waiting for you 🎁
                            </h2>

                            <p style="
                                margin:0;
                                color:#4b5563;
                                font-size:15px;
                                line-height:1.8;">
                                We're excited to let you know that a new promo code
                                is now available for your next subscription.
                            </p>

                            <!-- Promo Code -->
                            <div style="
                                margin:30px 0;
                                padding:22px;
                                background:#eff6ff;
                                border:2px dashed #2563eb;
                                border-radius:14px;
                                text-align:center;">

                                <p style="
                                    margin:0 0 8px 0;
                                    color:#64748b;
                                    font-size:13px;
                                    text-transform:uppercase;
                                    letter-spacing:1px;
                                    font-weight:600;">
                                    Promo Code
                                </p>

                                <div style="
                                    color:#1d4ed8;
                                    font-size:28px;
                                    font-weight:800;
                                    letter-spacing:2px;">
                                    {{result.Value.Code}}
                                </div>

                            </div>

                            <p style="
                                margin:0;
                                color:#4b5563;
                                font-size:15px;
                                line-height:1.8;">
                                Use this code when subscribing to the eligible plan
                                and enjoy your special discount.
                            </p>

                            <div style="
                                margin:30px 0 0 0;
                                padding:16px 18px;
                                background:#f8fafc;
                                border-radius:10px;
                                color:#64748b;
                                font-size:13px;
                                line-height:1.7;">

                                <strong style="color:#334155;">
                                    Don't miss out!
                                </strong>
                                <br>
                                This offer is available for a limited time and while
                                the available usage limit lasts.

                            </div>

                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style="
                            background:#f9fafb;
                            padding:25px;
                            text-align:center;
                            font-size:13px;
                            color:#9ca3af;">

                            Gym Management System<br>
                            © 2026 Gym Management System

                        </td>
                    </tr>

                </table>

            </td>
        </tr>

    </table>

</body>
</html>
""";

            notificationBackgroundJobs.SendToUsers(
            userIds,
            "New Promo Code 🎉",
            $"A new promo code {result.Value.Code} is now available.",
            NotificationType.General);

            var emails = await identityService.GetEmailsByPersonIdsAsync(personIds, ct);

            emailBackgroundJobs.SendToUsersAsync(
                emails.Value,
                "New Promo Code Available",
                htmlBody);

            return Result.Created;
        }
    }
}