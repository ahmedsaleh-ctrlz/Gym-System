namespace Gym.Infrastructure.Settings;

public sealed class AiSettings
{
    public const string SectionName = "AI";

    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
}