using CommentService.Contracts;

namespace CommentService.ProfanityFallback;

public interface ILocalProfanityValidator
{
    ProfanityCheckResponse Check(string content);
}