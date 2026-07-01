using System.Text.Json.Serialization;

namespace Gym.Infrastructure.Email;

internal sealed class BrevoEmailRequest
{
    [JsonPropertyName("sender")]
    public required BrevoSender Sender { get; init; }

    [JsonPropertyName("to")]
    public required List<BrevoRecipient> To { get; init; }

    [JsonPropertyName("subject")]
    public required string Subject { get; init; }

    [JsonPropertyName("htmlContent")]
    public required string HtmlContent { get; init; }
}

internal sealed class BrevoSender
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("email")]
    public required string Email { get; init; }
}

internal sealed class BrevoRecipient
{
    [JsonPropertyName("email")]
    public required string Email { get; init; }
}