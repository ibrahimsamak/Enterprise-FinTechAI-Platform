namespace FinAI.AI.Abstractions.Support;

public interface ICustomerSupportAgent
{
    IAsyncEnumerable<SupportStreamItem> AnswerAsync(string question, CancellationToken ct = default);
}

public sealed record SupportCitation(string DocId, string Section, string Title);

public sealed record SupportStreamItem(string? Delta, IReadOnlyList<SupportCitation>? Citations);
