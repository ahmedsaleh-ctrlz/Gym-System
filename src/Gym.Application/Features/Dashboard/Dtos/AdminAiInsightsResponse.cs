namespace Gym.Application.Features.Dashboard.Dtos;

public sealed record AdminAiInsightsResponse(
    string Summary,
    IReadOnlyList<AdminAiInsight> Insights,
    IReadOnlyList<AdminAiRecommendation> Recommendations,
    DateTime GeneratedAtUtc);

public sealed record AdminAiInsight(
    string Title,
    string Description,
    string Severity,
    string? Metric,
    string? Comparison);

public sealed record AdminAiRecommendation(
    string Title,
    string Description,
    string Priority);