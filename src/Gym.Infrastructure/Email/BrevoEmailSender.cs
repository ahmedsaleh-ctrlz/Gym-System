using System.Net.Http.Json;

using Gym.Application.Common.Interfaces;
using Gym.Infrastructure.Settings;

using Microsoft.Extensions.Options;

namespace Gym.Infrastructure.Email;

public sealed class BrevoEmailSender(
    HttpClient httpClient,
    IOptions<EmailSettings> emailSettings)
    : IEmailSender
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly EmailSettings _emailSettings = emailSettings.Value;

    public async Task SendEmailConfirmationAsync(
        string toEmail,
        string confirmationUrl,
        CancellationToken ct = default)
    {
        Console.WriteLine(_emailSettings.ApiKey);
        var request = new BrevoEmailRequest
        {
            Sender = new BrevoSender
            {
                Name = _emailSettings.FromName,
                Email = _emailSettings.FromEmail
            },
            To =
            [
                new BrevoRecipient
                {
                    Email = toEmail
                }
            ],
            Subject = "Confirm your email",
            HtmlContent = $$"""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                <meta charset="UTF-8">
                <title>Confirm your email</title>
                </head>

                <body style="margin:0;padding:0;background:#f3f6fb;font-family:Segoe UI,Arial,sans-serif;">

                <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="padding:40px 0;">
                <tr>
                <td align="center">

                <table role="presentation"
                    width="600"
                    cellspacing="0"
                    cellpadding="0"
                    style="
                            background:#ffffff;
                            border-radius:18px;
                            overflow:hidden;
                            box-shadow:0 10px 30px rgba(0,0,0,.08);">

                <tr>
                <td
                style="
                background:linear-gradient(135deg,#2563eb,#1d4ed8);
                padding:35px;
                text-align:center;">

                <div style="font-size:48px;">🏋️</div>

                <h1 style="color:white;margin:15px 0 0 0;">
                Gym Management System
                </h1>

                <p style="color:#dbeafe;font-size:16px;margin-top:10px;">
                Welcome to the Gym!
                </p>

                </td>
                </tr>

                <tr>
                <td style="padding:40px;">

                <h2 style="margin-top:0;color:#111827;">
                Confirm your email
                </h2>

                <p style="color:#4b5563;line-height:1.8;font-size:15px;">

                Thanks for creating your account.

                <br><br>

                Before you can start using Gym Management System,
                please verify your email address by clicking the button below.

                </p>

                <div style="text-align:center;margin:40px 0;">

                <a href="{{confirmationUrl}}"
                style="
                background:#2563eb;
                color:#ffffff;
                padding:16px 40px;
                text-decoration:none;
                border-radius:10px;
                display:inline-block;
                font-size:16px;
                font-weight:bold;">

                Confirm Email

                </a>

                </div>

                <div
                style="
                background:#f8fafc;
                border-left:4px solid #2563eb;
                padding:18px;
                border-radius:8px;
                font-size:14px;
                color:#4b5563;">

                If the button doesn't work,
                copy and paste the following link into your browser:

                <br><br>

                <a href="{{confirmationUrl}}" style="word-break:break-all;color:#2563eb;">
                {{confirmationUrl}}
                </a>

                </div>

                <p
                style="
                margin-top:35px;
                font-size:14px;
                color:#6b7280;">

                If you didn't create this account,
                you can safely ignore this email.

                </p>

                </td>
                </tr>

                <tr>
                <td
                style="
                background:#f9fafb;
                padding:25px;
                text-align:center;
                font-size:13px;
                color:#9ca3af;">

                © 2026 Gym Management System

                </td>
                </tr>

                </table>

                </td>
                </tr>
                </table>

                </body>
                </html>
                """
        };
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add(
            "api-key",
            _emailSettings.ApiKey);
        var response = await _httpClient.PostAsJsonAsync(
            "https://api.brevo.com/v3/smtp/email",
            request,
            ct);

        response.EnsureSuccessStatusCode();
    }
}