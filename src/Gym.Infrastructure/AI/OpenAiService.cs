#pragma warning disable OPENAI001

using Gym.Application.Common.Interfaces;
using Gym.Infrastructure.Settings;

using Microsoft.Extensions.Options;

using OpenAI.Responses;

namespace Gym.Infrastructure.AI;

public sealed class OpenAiService(
    ResponsesClient client,
    IOptions<AiSettings> settings)
    : IAiService
{
    private readonly string _model = settings.Value.Model;

    public async Task<string> GenerateInsightsAsync(
        string instructions,
        string analyticsContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instructions);
        ArgumentException.ThrowIfNullOrWhiteSpace(analyticsContext);

        var prompt = $"""
            {instructions}

            Analyze the following gym analytics data.
            Treat the supplied data as the only source of factual metrics.
            Do not invent numbers or claim causes that the data cannot establish.

            Analytics data:
            {analyticsContext}
            """;

        var response = await client.CreateResponseAsync(
            _model,
            prompt,
            cancellationToken: cancellationToken);

        var output = response.Value.GetOutputText();

        if (string.IsNullOrWhiteSpace(output))
        {
            throw new InvalidOperationException(
                "OpenAI returned an empty insights response.");
        }

        return output.Trim();
    }
}

#pragma warning restore OPENAI001