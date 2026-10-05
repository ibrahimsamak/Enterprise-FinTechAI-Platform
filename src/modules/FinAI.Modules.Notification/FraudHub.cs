using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
namespace FinAI.Modules.Notification;

[Authorize(Policy = "FraudOfficer")]
public sealed class FraudHub : Hub { }

public sealed class FraudFlaggedRelayHandler(IHubContext<FraudHub> hub): IIntegrationEventHandler<FraudFlaggedIntegrationEvent>
{
     public Task HandleAsync(FraudFlaggedIntegrationEvent e, CancellationToken ct) => hub.Clients.All.SendAsync("fraudFlagged", e, ct);
}

