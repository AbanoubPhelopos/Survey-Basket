using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Survey_Basket.Infrastructure.Data;
using Xunit;

namespace Survey_Basket.Tests.Abstractions;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // Remove ALL EF Core / Npgsql / DbContext registrations aggressively
            var descriptorsToRemove = services
                .Where(d =>
                    d.ServiceType.FullName?.Contains("Npgsql") == true ||
                    d.ImplementationType?.FullName?.Contains("Npgsql") == true ||
                    d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                    (d.ServiceType.IsGenericType &&
                     d.ServiceType.GetGenericTypeDefinition().FullName?.Contains("DbContextOptions") == true))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
                services.Remove(descriptor);

            // Remove Hangfire hosted services to prevent PostgreSQL connection
            var hangfireHostedServices = services
                .Where(d =>
                    d.ServiceType == typeof(IHostedService) &&
                    (d.ImplementationType?.FullName?.Contains("Hangfire") == true ||
                     d.ImplementationFactory?.Method.DeclaringType?.FullName?.Contains("Hangfire") == true))
                .ToList();

            foreach (var descriptor in hangfireHostedServices)
                services.Remove(descriptor);

            // Re-add DbContext with InMemory provider
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting_" + Guid.NewGuid());
            });

            // Provide a fake IHttpContextAccessor with a valid user 
            // (ApplicationDbContext.SaveChangesAsync reads NameIdentifier for auditing)
            services.RemoveAll<IHttpContextAccessor>();
            services.AddSingleton<IHttpContextAccessor>(_ =>
            {
                var context = new DefaultHttpContext();
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new(ClaimTypes.Name, "TestUser"),
                };
                context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
                return new HttpContextAccessor { HttpContext = context };
            });

            // Override authentication: set "Test" as the default for all scheme purposes
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                    options.DefaultScheme = "Test";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
                options.DefaultScheme = "Test";
            });
        });
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public new Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
