namespace FinAI.AI.Rag;

public sealed record RetrievedChunk(string DocId, string Section, string Title, string Heading, string Text, double Distance)
{
    public string Citation => $"{DocId} §{Section}";
    public string ToPromptText() => $"[{Citation}] {Title} - {Heading}\n{Text}";
}
