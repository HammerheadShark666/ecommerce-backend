using System.Text;
using ECommerce.Infrastructure.Configurations;
using ECommerce.Infrastructure.Library.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.Infrastructure.Extensions;

public static class JwtExtension
{
    public static void AddJwtExtension(this IServiceCollection services, JwtOptions jwtOptions) => services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer ?? Constants.ProjectName,
                    ValidAudience = jwtOptions.Audience ?? Constants.ProjectName, 
                    IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtOptions.Secret))
                    {
                        KeyId = jwtOptions.KeyId
                    },

                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();

                        HttpContext http = context.HttpContext;
                        if (!http.Response.HasStarted)
                        {
                            http.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            await http.Response.WriteAsJsonAsync(new ProblemDetails
                            {
                                Status = StatusCodes.Status401Unauthorized,
                                Title = "Unauthorized",
                                Detail = context.ErrorDescription
                                    ?? context.Error
                                    ?? "Authentication failed"
                            });
                        }
                    },
                    OnAuthenticationFailed = async context =>
                    {
                        HttpContext http = context.HttpContext;
                        if (!http.Response.HasStarted)
                        {
                            http.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            await http.Response.WriteAsJsonAsync(new ProblemDetails
                            {
                                Status = StatusCodes.Status401Unauthorized,
                                Title = "Authentication Failed",
                                Detail = context.Exception?.Message
                            });
                        }
                    }
                };
            });
}
