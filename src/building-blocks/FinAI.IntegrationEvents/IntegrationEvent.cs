using System.Diagnostics;
using System.Text.Json.Serialization;

public abstract record IntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Activity.Current?.TraceId.ToString() ?? "";

    [JsonIgnore] public virtual string PartitionKey => Id.ToString();
}

