using PublisherService.Contracts;

namespace PublisherService.Clients;

public class ProfanityHttpClient : IProfanityClient
{
    private readonly HttpClient _httpClient;

    public ProfanityHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProfanityCheckResponse> CheckTextAsync(string text, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "/profanity/check",
            new ProfanityCheckRequest { Text = text },
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ProfanityCheckResponse>(
            cancellationToken: cancellationToken);

        return result ?? throw new InvalidOperationException("No response from ProfanityService.");
    }
}