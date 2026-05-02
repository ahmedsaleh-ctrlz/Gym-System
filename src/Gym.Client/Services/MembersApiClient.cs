using System.Net.Http.Json;
using System.Text.Json;
using Gym.Client.Models;

namespace Gym.Client.Services;

public sealed class MembersApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<PaginatedList<MemberItem>> GetMembersAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchTerm = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default)
    {
        var queryParts = new List<string>
        {
            $"pageNumber={pageNumber}",
            $"pageSize={pageSize}"
        };

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            queryParts.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
        }

        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            queryParts.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
        }

        if (!string.IsNullOrWhiteSpace(sortDirection))
        {
            queryParts.Add($"sortDirection={Uri.EscapeDataString(sortDirection)}");
        }

        var url = $"api/members?{string.Join("&", queryParts)}";
        return await httpClient.GetFromJsonAsync<PaginatedList<MemberItem>>(url, JsonOptions, cancellationToken)
            ?? new PaginatedList<MemberItem> { Items = [] };
    }

    public async Task<MemberItem> CreateMemberAsync(CreateMemberModel model, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/members", model, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<MemberItem>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("The API returned an empty member payload.");
    }

    public async Task UpdateMemberAsync(int memberId, UpdateMemberModel model, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            model.FirstName,
            model.LastName,
            model.DateOfBirth,
            model.PhoneNumber,
            model.JoinDate,
            model.Notes
        };

        var response = await httpClient.PutAsJsonAsync($"api/members/{memberId}", payload, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task UpdateMemberImageAsync(int memberId, string imageUrl, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync($"api/members/{memberId}/image", new { imageUrl }, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task DeleteMemberAsync(int memberId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"api/members/{memberId}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>(JsonOptions, cancellationToken);
        var message = problem?.Detail
            ?? problem?.Title
            ?? $"{(int)response.StatusCode} {response.ReasonPhrase}";

        throw new InvalidOperationException(message);
    }
}
