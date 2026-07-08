using Serilog;
using ECommerce.Api.Extensions;
using ECommerce.Api.Middleware;
using ECommerce.Api.Middleware.ExceptionHandlers;
using ECommerce.Application;
using ECommerce.Application.Features.Authentication.Login;
using ECommerce.Application.Features.Authentication.Protected;
using ECommerce.Application.Features.Authentication.VerifyTwoFactorLogin;
using ECommerce.Application.Features.CheckTwoFactorStatus;
using ECommerce.Application.Features.RefreshToken;
using ECommerce.Application.Features.Registration;
using ECommerce.Application.Features.TwoFactorEnrolment;
using ECommerce.Infrastructure;
using ECommerce.Infrastructure.Extensions;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information(">>> PROGRAM STARTED");

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args); 

    builder.AddAppSettings();
    builder.AddApplicationLogging();
    builder.Services.AddAllOptions(builder.Configuration);
    builder.Services.AddOpenApi();
    builder.Services.AddAuthorization();
    builder.Services.AddExceptionHandler<UnauthorizedExceptionHandler>();
    builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
    builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();
    builder.AddKeyVaultExtension();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplication(builder.Configuration);

    WebApplication app = builder.Build();

    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseExceptionHandler();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseSerilogRequestLogging(options => options.EnrichDiagnosticContext = (diag, httpContext) => diag.Set("CorrelationId", httpContext.TraceIdentifier));

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.MapLoginEndpoints();
    app.MapTwoFactorStatusEndpoints();
    app.MapVerifyTwoFactorLoginEndpoints();
    app.MapTwoFactorEnrolmentEndpoints();
    app.MapRegistrationEndpoints();
    app.MapRefreshTokenEndPoints();
    app.MapProtectedEndpoints();

    Log.Information(">>> BEFORE RUN");

    app.Run();

    Log.Information("RUN EXITED");

}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
