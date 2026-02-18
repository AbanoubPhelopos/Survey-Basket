using Hangfire;
using Microsoft.Extensions.Hosting;
using HangfireBasicAuthenticationFilter;
using Microsoft.OpenApi.Models;
using Serilog;
using Survey_Basket.Application.Services.NotificationServices;
using Survey_Basket.Application.Settings;
using Survey_Basket.Application;
using Survey_Basket.Infrastructure;


Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up the host...");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, configuration)
                => configuration.ReadFrom.Configuration(context.Configuration));

    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.Configure<MailSettings>(builder.Configuration.GetSection(nameof(MailSettings)));

    builder.Services.AddControllers();

    builder.Services.AddSwaggerGen(options =>

    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "Survey Basket API", Version = "v1" });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter a valid token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();

        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Survey Basket API V1");
        });
    }


    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();

    app.UseHangfireDashboard("/jobs", new DashboardOptions
    {
        DashboardTitle = "Survey Basket - Background Jobs",
        Authorization = [
            new HangfireCustomBasicAuthenticationFilter
            {
                User = app.Configuration.GetValue<string>("HangfireSettings:username"),
                Pass = app.Configuration.GetValue<string>("HangfireSettings:password")
            }
        ],
        //IsReadOnlyFunc = (DashboardContext context) => true
    });


    var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
    using var scope = scopeFactory.CreateScope();
    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

    RecurringJob.AddOrUpdate(
        "send-daily-poll-notification",
        () => notificationService.SendNewPollsNotification(null),
        Cron.Daily
    );


    app.UseCors();

    app.UseAuthentication();

    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
