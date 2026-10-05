using FinAI.AI.Abstractions.Support;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FinAI.Modules.Notification;

[Authorize(Policy = "Customer")]
public sealed class SupportHub(ICustomerSupportAgent agent) : Hub
{
    private const int MaxQuestionLength = 1000;

    public IAsyncEnumerable<SupportStreamItem> Ask(string question, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(question) || question.Length > MaxQuestionLength)
            throw new HubException($"Ask a question of 1-{MaxQuestionLength} characters.");

        return agent.AnswerAsync(question.Trim(), ct);
    }
}
