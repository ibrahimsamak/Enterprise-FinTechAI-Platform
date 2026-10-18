using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;

namespace FinAI.Modules.Identity;

public static class AuthExtensions
{
    public static IServiceCollection AddFinAIAuth(this IServiceCollection s, IConfiguration cfg)
    {
        var auth = s.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);

        if (cfg.GetSection("AzureAd").Exists())
        {
            auth.AddMicrosoftIdentityWebApi(
                jwt =>
                {
                    cfg.Bind("AzureAd", jwt);
                    jwt.Events = new JwtBearerEvents { OnMessageReceived = ReadHubToken };
                },
                identity => cfg.Bind("AzureAd", identity));
            s.AddMemoryCache();
            s.AddTransient<IClaimsTransformation, CustomerClaimTransformation>();
        }
        else
        {
            auth.AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = cfg["JWT:ISSUER"],
                    ValidAudience = cfg["JWT:AUDIENCE"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["JWT:SIGNINGKEY"]!)),
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true
                };
                options.Events = new JwtBearerEvents { OnMessageReceived = ReadHubToken };
            });
        }

        s.AddAuthorizationBuilder()
            .AddPolicy("FraudOfficer", p => p.RequireRole("FraudOfficer"))
            .AddPolicy("Customer", p => p.RequireRole("Customer"))
            .AddPolicy("ComplianceOfficer", p => p.RequireRole("ComplianceOfficer"))
            .AddPolicy("Admin", p => p.RequireRole("Admin"));
        return s;
    }

    private static Task ReadHubToken(MessageReceivedContext ctx)
    {
        var token = ctx.Request.Query["access_token"];
        if (!string.IsNullOrEmpty(token) && ctx.HttpContext.Request.Path.StartsWithSegments("/hubs"))
            ctx.Token = token;
        return Task.CompletedTask;
    }
}
