using API.Middleware;
using Core.Entities;
using API.SignalR;
using API.Extensions;
using Scalar.AspNetCore;
using API.OpenApi;

var builder = WebApplication.CreateBuilder(args);


if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApi(options =>
    {
        options.AddOperationTransformer<CommonResponsesTransformer>();
    });
}


builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGroup("api")
    .MapIdentityApi<AppUser>()
    .ExcludeFromDescription();
    
app.MapHub<NotificationHub>("hub/notifications");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

await app.ApplyMigrationsAndSeedAsync();

app.Run();
