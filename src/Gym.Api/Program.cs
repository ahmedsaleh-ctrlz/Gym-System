using Gym.Api;
using Gym.Application;
using Gym.Application.Common.Behaviors;
using Gym.Application.Features.Members.Commands.CreateMember;
using Gym.Infrastructure;
using Gym.Infrastructure.Data;

using Scalar.AspNetCore;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

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

app.MapGet("/", () => "Api Is Running");

app.Run();