using DotNetEnv;

using Gym.Api;
using Gym.Application;
using Gym.Application.Common.Behaviors;
using Gym.Application.Features.Members.Commands.CreateMember;
using Gym.Infrastructure;
using Gym.Infrastructure.Data;
using Gym.Infrastructure.Hubs;

using QuestPDF.Infrastructure;

using Scalar.AspNetCore;

using Serilog;

Env.Load("../../.env");
var builder = WebApplication.CreateBuilder(args);
QuestPDF.Settings.License = LicenseType.Evaluation;

builder.Services.AddApi(builder.Configuration)
    .AddApplicaiton()
    .AddInfrastructure(builder.Configuration);
builder.Host.UseSerilog((context, loggerConfigration) =>
{
    loggerConfigration.ReadFrom.Configuration(builder.Configuration);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Gym API V1");
        options.SwaggerEndpoint("/openapi/v2.json", "Gym API V2");
        options.EnableDeepLinking();
        options.DisplayRequestDuration();
        options.EnableFilter();
    });

    app.MapScalarApiReference();
    await app.InitialiseDatabaseAsync();
}
else
{
    app.UseHsts();
}

app.UseCoreMiddlewares(builder.Configuration);

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.MapGet("/", () => "Api Is Running");

app.Run();