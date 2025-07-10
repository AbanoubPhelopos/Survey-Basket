using FluentValidation;
using FluentValidation.AspNetCore;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        ///Registering the Identity services
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>().AddEntityFrameworkStores<ApplicationDbContext>();


        return services;
    }
}
