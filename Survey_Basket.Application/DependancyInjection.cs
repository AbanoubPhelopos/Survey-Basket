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
using Survey_Basket.Application.Implementation;
using Survey_Basket.Domain.Models;
using Survey_Basket.Infrastructure.Data;
using Survey_Basket.Infrastructure.Implementation;
using System.Reflection;

namespace Survey_Basket.Infrastructure;

public static class DependancyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
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
                ValidIssuer = "Survey_Basket",
                ValidAudience = "Survey_Basket users",
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("gwvOEFH1SAscazoGhTAOtBxJR8Zn0jaH"))
            };
        });


        ///Registering the Identity services
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>().AddEntityFrameworkStores<ApplicationDbContext>();



        return services;
    }
}
