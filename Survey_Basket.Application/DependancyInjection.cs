using FluentValidation;
using FluentValidation.AspNetCore;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Survey_Basket.Application.Abstraction;
using Survey_Basket.Application.Contracts.Authentication;
using Survey_Basket.Application.Implementation;
using Survey_Basket.Domain.Models;
using Survey_Basket.Infrastructure.Data;
using Survey_Basket.Infrastructure.Implementation;
using System.Reflection;
using System.Text;

namespace Survey_Basket.Infrastructure;

public static class DependancyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        /// Registering the HttpContextAccessor
        var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                builder =>
                {
                    builder.WithOrigins(allowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
        });


        /// Registering Mapster for object mapping
        var mappingConfig = Mapster.TypeAdapterConfig.GlobalSettings;
        mappingConfig.Scan(AppDomain.CurrentDomain.GetAssemblies());
        services.AddSingleton<IMapper>(new Mapper(mappingConfig));

        /// Registering FluentValidation services
        services
            .AddFluentValidationAutoValidation()
            .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        /// Registering the DbContext with PostgreSQL
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        /// Registering the application services 
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPollService, PollService>();



        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var jwtSettings = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();

        services.AddSingleton<IJwtProvider, JwtProvider>();
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(o =>
        {
            o.SaveToken = true;
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings?.Issuer!,
                ValidAudience = jwtSettings?.Audience!,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.Key!))
            };
        });


        ///Registering the Identity services
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>().AddEntityFrameworkStores<ApplicationDbContext>();

        return services;
    }
}
