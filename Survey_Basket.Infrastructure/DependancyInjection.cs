using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Survey_Basket.Application.Abstraction;
using Survey_Basket.Infrastructure.Data;
using Survey_Basket.Infrastructure.Implementation;

namespace Survey_Basket.Infrastructure;

public static class DependancyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {

        var mappingConfig = Mapster.TypeAdapterConfig.GlobalSettings;
        mappingConfig.Scan(AppDomain.CurrentDomain.GetAssemblies());
        services.AddSingleton<IMapper>(new Mapper(mappingConfig));


        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IPollService, PollService>();

        return services;
    }
}
