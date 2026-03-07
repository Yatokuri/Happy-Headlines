using CommentService.Contracts;

namespace CommentService.Clients;

public class ProfanityHttpClient(HttpClient httpClient) : IProfanityClient
{
    public async Task<ProfanityCheckResponse> CheckTextAsync(string text, CancellationToken cancellationToken)
    {
        var request = new ProfanityCheckRequest
        {
            Text = text
        };

        var response = await httpClient.PostAsJsonAsync("/profanity/check", request, cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ProfanityCheckResponse>(cancellationToken: cancellationToken);

        return result ?? throw new InvalidOperationException("No response returned from ProfanityService.");
    }
}