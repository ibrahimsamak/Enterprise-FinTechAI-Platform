using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace FinAI.Modules.Identity;

public static class AuthExtensions
{
    public static IServiceCollection AddFinAIAuth(this IServiceCollection s, IConfiguration cfg)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["JWT:SIGNINGKEY"]!));
        s.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = cfg["JWT:ISSUER"],
                    ValidAudience = cfg["JWT:AUDIENCE"],
                    IssuerSigningKey = key,
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        var token = ctx.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(token) && ctx.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                            ctx.Token = token;
                        return Task.CompletedTask;
                    }
                };
            });
        s.AddAuthorizationBuilder()
            .AddPolicy("FraudOfficer", p => p.RequireRole("FraudOfficer"))
            .AddPolicy("Customer", p => p.RequireRole("Customer"));
        return s;
    }
}
