using Microsoft.AspNetCore.Mvc;

namespace API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services, 
        IConfiguration config)
    {
        services.AddControllers(options =>
        {
            options.Filters
                .Add(new ProducesAttribute("application/json"));
        });

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        services.AddSignalR();

        return services;
    }
}
