using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

using Asp.Versioning;

using Gym.Api.GraphQL;
using Gym.Api.Infrastructure;
using Gym.Api.OpenApi;
using Gym.Api.Services;
using Gym.Application.Common.Interfaces;
using Gym.Infrastructure.BackgroundJobs;
using Gym.Infrastructure.Settings;

using Hangfire;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.FileProviders;

using Serilog;
namespace Gym.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services
        .AddControllerWithJsonConfiguration()
        .AddConfiguredCors(configuration)
        .AddCustomProblemDetails()
        .AddCustomRateLimiting()
        .AddCustomApiVersioning()
        .AddCustomerExceptionHandling()
        .AddApiDocumentation()
        .AddIdentityInfrastructure();

        services.AddHealthChecks().AddCheck<Infrastructure.DatabaseHealthCheck>("database");
        services
            .AddGraphQLServer()
            .AddAuthorization()
            .AddQueryType<Query>()
            .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = false);

        services.Configure<StripeSettings>(configuration.GetSection("Stripe"));
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });
        services
            .AddAuthentication()
            .AddCookie(IdentityConstants.ExternalScheme, options =>
            {
                options.Cookie.Name = "Gym.External";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                options.SlidingExpiration = false;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            })
            .AddGoogle("Google", options =>
            {
                options.ClientId = configuration["Authentication:Google:ClientId"]
                    ?? configuration["Google:ClientId"]
                    ?? string.Empty;
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"]
                    ?? configuration["Google:ClientSecret"]
                    ?? string.Empty;
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.CallbackPath = "/signin-google";
                options.ClaimActions.MapJsonKey(
                "email_verified",
                "email_verified",
                ClaimValueTypes.Boolean);
            });
        return services;
    }

    public static IServiceCollection AddConfiguredCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            var allowedOrigins = configuration.GetSection("AppSettings:AllowedOrigins").Get<string[]>();
            options.AddPolicy("Gym", policy =>
            {
                policy.WithOrigins(allowedOrigins!)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
            });
        });

        return services;
    }

    public static IServiceCollection AddCustomProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options => options.CustomizeProblemDetails = (context) =>
        {
            context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
            context.ProblemDetails.Extensions.Add("requestId", context.HttpContext.TraceIdentifier);
        });

        return services;
    }

    public static IServiceCollection AddCustomApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddMvc()
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });
        return services;
    }

    public static IServiceCollection AddControllerWithJsonConfiguration(this IServiceCollection services)
    {
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions
                .DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        return services;
    }

    public static IServiceCollection AddCustomerExceptionHandling(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        return services;
    }

    public static IServiceCollection AddCustomRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                var httpContext = context.HttpContext;
                httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                httpContext.Response.ContentType = "application/problem+json";

                int? retryAfterSeconds = null;
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    var seconds = (int)Math.Max(1, Math.Ceiling(retryAfter.TotalSeconds));
                    retryAfterSeconds = seconds;
                    httpContext.Response.Headers.RetryAfter = seconds.ToString(CultureInfo.InvariantCulture);
                }

                var partitionKey = GetClientPartitionKey(httpContext);
                var logger = httpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("Gym.Api.RateLimiting");

                logger.LogWarning(
                    "Rate limit exceeded for client {PartitionKey} on {Method} {Path}. Retry-After: {RetryAfter}s",
                    partitionKey,
                    httpContext.Request.Method,
                    httpContext.Request.Path,
                    retryAfterSeconds);

                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Too Many Requests",
                    Detail = "You have exceeded your request rate limit. Please try again later.",
                    Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
                };

                problemDetails.Extensions.Add("requestId", httpContext.TraceIdentifier);
                if (retryAfterSeconds.HasValue)
                {
                    problemDetails.Extensions.Add("retryAfter", retryAfterSeconds.Value);
                }

                await httpContext.Response.WriteAsJsonAsync(
                    problemDetails,
                    cancellationToken: cancellationToken);
            };

            // Global Limiter (100 requests per minute per UserId or IP)
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                return RateLimitPartition.GetSlidingWindowLimiter(
                    GetClientPartitionKey(httpContext),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 6,
                        QueueLimit = 0
                    });
            });

            // GlobalLimiter named policy for explicit endpoint binding
            options.AddPolicy("GlobalLimiter", httpContext =>
            {
                return RateLimitPartition.GetSlidingWindowLimiter(
                    GetClientPartitionKey(httpContext),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 6,
                        QueueLimit = 0
                    });
            });

            // LoginRateLimiter (Auth & Security: 5 requests per 1 minute per IP)
            options.AddPolicy("LoginRateLimiter", httpContext =>
            {
                var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetSlidingWindowLimiter(
                    $"login:{ip}",
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 6,
                        QueueLimit = 0
                    });
            });

            // resend-email (External Quota Protection: 1 request per 60 seconds per User/IP)
            options.AddPolicy("resend-email", httpContext =>
            {
                return RateLimitPartition.GetTokenBucketLimiter(
                    $"email:{GetClientPartitionKey(httpContext)}",
                    _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 1,
                        TokensPerPeriod = 1,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(60),
                        AutoReplenishment = true,
                        QueueLimit = 0
                    });
            });

            // HeavyOperations (CPU/IO/Payment Protection: 10 requests per minute per User/IP)
            options.AddPolicy("HeavyOperations", httpContext =>
            {
                return RateLimitPartition.GetSlidingWindowLimiter(
                    $"heavy:{GetClientPartitionKey(httpContext)}",
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 6,
                        QueueLimit = 0
                    });
            });
        });

        return services;
    }

    [Obsolete("Use AddCustomRateLimiting instead.")]
    public static IServiceCollection AddCustomeRateLimiting(this IServiceCollection services)
    {
        return services.AddCustomRateLimiting();
    }

    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUser, CurrentUser>();
        services.AddHttpContextAccessor();
        return services;
    }

    public static IApplicationBuilder UseCoreMiddlewares(this IApplicationBuilder app, IConfiguration configuration)
    {
        var forwardedHeadersOptions = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        };
        forwardedHeadersOptions.KnownIPNetworks.Clear();
        forwardedHeadersOptions.KnownProxies.Clear();
        app.UseForwardedHeaders(forwardedHeadersOptions);

        app.UseExceptionHandler();
        app.UseStatusCodePages();
        app.UseHttpsRedirection();
        app.UseCors("Gym");
        app.UseStaticFiles();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();
        app.UseBackgroundJobs();
        app.UseCustomStaticFiles();
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = [new HangfireAuthorizationFilter()]
        });
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        });

        return app;
    }

    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        string[] versions = ["v1", "V2"];

        foreach (var version in versions)
        {
            services.AddOpenApi(version, options =>
            {
                // Versioning config
                options.AddDocumentTransformer<VersionInfoTransformer>();
                options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            });
        }

        return services;
    }

    public static IApplicationBuilder UseBackgroundJobs(
    this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();

        var recurringJobManager =
            scope.ServiceProvider
                .GetRequiredService<IRecurringJobManager>();

        JobScheduler.RegisterRecurringJobs(
            recurringJobManager);

        return app;
    }

    public static IApplicationBuilder UseCustomStaticFiles(
    this IApplicationBuilder app)
    {
        var uploadsPath = System.IO.Path.Combine(
            Directory.GetCurrentDirectory(),
            "Uploads");

        Directory.CreateDirectory(uploadsPath);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(uploadsPath),
            RequestPath = "/Uploads",
            OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                ctx.Context.Response.Headers.Append("Content-Security-Policy", "default-src 'none'");
            }
        });

        return app;
    }

    private static string GetClientPartitionKey(HttpContext httpContext)
    {
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? httpContext.User.FindFirstValue("sub");

            if (!string.IsNullOrWhiteSpace(userId))
            {
                return $"user:{userId}";
            }
        }

        var ip = httpContext.Connection.RemoteIpAddress?.ToString();
        return $"ip:{(!string.IsNullOrWhiteSpace(ip) ? ip : "unknown")}";
    }
}