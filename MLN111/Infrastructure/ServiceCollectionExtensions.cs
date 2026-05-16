using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MLN111.Data;
using MLN111.Services.Ai;
using MLN111.Services.Auth;
using MLN111.Services.Health;
using MLN111.Services.Quiz;

namespace MLN111.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = DatabaseConnection.Resolve(configuration);
        services.AddDbContext<AppDbContext>(opts => opts.UseNpgsql(connectionString));

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<SeedAdminOptions>(configuration.GetSection(SeedAdminOptions.SectionName));
        services.Configure<AiOptions>(configuration.GetSection(AiOptions.SectionName));

        services.AddHttpClient("Ai", client => client.Timeout = TimeSpan.FromMinutes(3));
        services.AddScoped<IAiChatService, AiChatService>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opts =>
            {
                var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
                };
            });

        services.AddAuthorization();

        services.AddScoped<IDatabaseHealthService, DatabaseHealthService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IQuizRoomService, QuizRoomService>();

        services.Configure<CorsOptions>(configuration.GetSection(CorsOptions.SectionName));
        var cors = configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();
        services.AddCors(opts =>
        {
            opts.AddDefaultPolicy(p =>
            {
                if (cors.AllowAnyOrigin)
                    p.AllowAnyOrigin();
                else if (cors.AllowedOrigins.Length > 0)
                    p.WithOrigins(cors.AllowedOrigins);
                else
                    p.WithOrigins("http://localhost:5173", "http://localhost:3000");

                p.AllowAnyHeader().AllowAnyMethod();
            });
        });

        return services;
    }

    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "MLN111 API", Version = "v1" });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Nhap: Bearer {token}"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
        });
        return services;
    }
}
