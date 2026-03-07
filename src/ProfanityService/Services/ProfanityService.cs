using Microsoft.EntityFrameworkCore;
using ProfanityService.Contracts;
using ProfanityService.Data;
using ProfanityService.Models;

namespace ProfanityService.Services;

public class ProfanityService(ProfanityDbContext dbContext) : IProfanityService
{
    public async Task<CheckProfanityResponse> CheckTextAsync(string text, CancellationToken cancellationToken)
    {
        var normalizedText = text.Trim().ToLowerInvariant();

        var words = await dbContext.ProfanityWords
            .Select(x => x.Word)
            .ToListAsync(cancellationToken);

        var matchedWords = words
            .Where(word => normalizedText.Contains(word.ToLowerInvariant()))
            .Distinct()
            .ToList();

        return new CheckProfanityResponse
        {
            ContainsProfanity = matchedWords.Count > 0,
            MatchedWords = matchedWords
        };
    }

    public async Task<ProfanityWordResponse> AddWordAsync(string word, CancellationToken cancellationToken)
    {
        var normalized = word.Trim().ToLowerInvariant();

        var exists = await dbContext.ProfanityWords
            .AnyAsync(x => x.Word == normalized, cancellationToken);

        if (exists)
            throw new InvalidOperationException("Word already exists.");

        var entity = new ProfanityWord
        {
            Id = Guid.NewGuid(),
            Word = normalized,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.ProfanityWords.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ProfanityWordResponse
        {
            Id = entity.Id,
            Word = entity.Word,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }

    public async Task<IReadOnlyCollection<ProfanityWordResponse>> GetWordsAsync(CancellationToken cancellationToken)
    {
        return await dbContext.ProfanityWords
            .OrderBy(x => x.Word)
            .Select(x => new ProfanityWordResponse
            {
                Id = x.Id,
                Word = x.Word,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);
    }
}