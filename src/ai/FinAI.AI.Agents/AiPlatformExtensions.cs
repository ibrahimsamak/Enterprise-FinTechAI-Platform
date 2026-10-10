using FinAI.AI.Abstractions.Fraud;
using FinAI.AI.Abstractions.Support;
using FinAI.AI.Agents.Fraud;
using FinAI.AI.Agents.Support;
using FinAI.AI.Rag;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinAI.AI.Agents;

public static class AiPlatformExtensions
{
    public static IServiceCollection AddFinAiAiCore(this IServiceCollection s, IConfiguration cfg)
    {
        var openai = new OpenAI.OpenAIClient(cfg["OPENAI:APIKEY"]);
        s.AddSingleton<IChatClient>(_ =>
            new ChatClientBuilder(openai.GetChatClient(cfg["OPENAI:CHATMODEL"]).AsIChatClient())
            .UseFunctionInvocation()
            .Build());

        s.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(_ =>
                openai.GetEmbeddingClient(cfg["OPENAI:EMBEDDINGMODEL"]).AsIEmbeddingGenerator());
        s.AddFinAiRag(cfg);
        return s;
    }

    public static IServiceCollection AddFinAiAiPlatform(this IServiceCollection s, IConfiguration cfg)
    {
        s.AddFinAiAiCore(cfg);
        s.AddScoped<IFraudAgent, FraudAgent>();
        s.AddScoped<FraudTools>();
        return s;
    }

    public static IServiceCollection AddFinAiCustomerSupportAgent(this IServiceCollection s) =>
        s.AddScoped<ICustomerSupportAgent, CustomerSupportAgent>();
}
