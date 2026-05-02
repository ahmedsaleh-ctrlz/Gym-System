using Gym.Application.Common.Behaviors;
using Gym.Application.Features.Members.Commands.CreateMember;
using Gym.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Client", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddHybridCache();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateMemberCommand).Assembly);
    cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
    cfg.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
});
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

await app.Services.InitialiseInfrastructureAsync();

app.UseHttpsRedirection();
app.UseCors("Client");
app.MapControllers();
app.MapGet("/", () => Results.Ok("Gym API is running."));

app.Run();
