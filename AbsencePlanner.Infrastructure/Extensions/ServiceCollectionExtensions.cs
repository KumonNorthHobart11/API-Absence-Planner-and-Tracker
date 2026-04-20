using System.Text;
using AbsencePlanner.Core.Configuration;
using AbsencePlanner.Core.Interfaces;
using AbsencePlanner.Infrastructure.Repositories;
using AbsencePlanner.Infrastructure.Services;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace AbsencePlanner.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
    {
        // Bind all config sections — IOptionsMonitor<T> auto-reloads on appsettings change
        services.Configure<FirebaseSettings>(config.GetSection("Firebase"));
        services.Configure<JwtSettings>(config.GetSection("Jwt"));
        services.Configure<SmtpSettings>(config.GetSection("Smtp"));
        services.Configure<TwilioSettings>(config.GetSection("Twilio"));
        services.Configure<OtpSettings>(config.GetSection("Otp"));
        services.Configure<AppSettings>(config.GetSection("App"));

        // Firebase — factory pattern for dynamic database reconnection
        services.AddSingleton<FirestoreDbFactory>();
        services.AddSingleton<IFirestoreRepository, FirestoreRepository>();

        // JWT — reads IOptionsMonitor<JwtSettings> at startup for middleware config,
        // but JwtService uses IOptionsMonitor for dynamic token generation
        var jwtSettings = config.GetSection("Jwt").Get<JwtSettings>()!;
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
                };
            });

        // Services — auto-register via Scrutor assembly scanning
        services.Scan(scan => scan
         .FromAssemblyOf<SeedService>()    // AbsencePlanner.Infrastructure assembly
    .AddClasses(classes => classes.InNamespaceOf<SeedService>())
     .AsMatchingInterface()          // IAuthService ? AuthService, etc.
     .WithSingletonLifetime());

        // SeedService has no matching interface — register explicitly
        services.AddSingleton<SeedService>();

        return services;
    }
}
