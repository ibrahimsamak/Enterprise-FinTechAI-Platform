using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http;
using StackExchange.Redis;

namespace FinAI.BuildingBlocks.Application.Behaviors;

public sealed class IdempotencyBehavior<TReq, TRes>(IConnectionMultiplexer redis, IHttpContextAccessor http, ICurrentUser user)
    : IPipelineBehavior<TReq, TRes> where TReq : notnull
{
    private const string InProgress = "__in_progress__";
    private static readonly TimeSpan LockTtl = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan ResultTtl = TimeSpan.FromHours(24);

    public async Task<TRes> Handle(TReq req, RequestHandlerDelegate<TRes> next, CancellationToken ct)
    {
        var key = http.HttpContext?.Request.Headers["Idempotency-Key"].FirstOrDefault();
        if (key is null) return await next();

        var caller = user.UserId?.ToString() ?? "anonymous";
        var redisKey = $"idem:{caller}:{typeof(TReq).FullName}:{key}";
        var db = redis.GetDatabase();

        if (!await db.StringSetAsync(redisKey, InProgress, LockTtl, When.NotExists))
        {
            var cached = await db.StringGetAsync(redisKey);
            if (cached == InProgress) throw new IdempotencyConflictException(key);
            if (cached.HasValue) return JsonSerializer.Deserialize<TRes>(cached.ToString())!;
            throw new IdempotencyConflictException(key);
        }

        TRes result;
        try
        {
            result = await next();
        }
        catch
        {
            await db.KeyDeleteAsync(redisKey);
            throw;
        }

        await db.StringSetAsync(redisKey, JsonSerializer.Serialize(result), ResultTtl);
        return result;
    }
}

public sealed class IdempotencyConflictException(string key)
    : Exception($"A request with Idempotency-Key '{key}' is already in progress.");
