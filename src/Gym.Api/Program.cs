using Gym.Api;
using Gym.Application;
using Gym.Application.Common.Behaviors;
using Gym.Application.Features.Members.Commands.CreateMember;
using Gym.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApi()
    .AddApplicaiton()
    .AddInfrastructure(builder.Configuration);


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
}
else
{
    app.UseHsts();
}

app.UseCoreMiddlewares();

app.MapControllers();

app.MapGet("/", () => "Api Is Running");


app.Run();
