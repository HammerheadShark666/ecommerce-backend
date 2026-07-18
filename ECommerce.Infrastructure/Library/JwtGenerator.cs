using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerce.Application.Abstractions;
using ECommerce.Domain.Entities.User;
using ECommerce.Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.Infrastructure.Library;

public class JwtGenerator(IOptions<JwtOptions> jwtOptions) : IJwtGenerator
{
    public async Task<string> GenerateTokenAsync(User user)
    { 
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.Secret))
        {
            KeyId = jwtOptions.Value.KeyId
        };
        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256); 

        Claim[] claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
        ];
               
        var token = new JwtSecurityToken(
            issuer: jwtOptions.Value.Issuer,
            audience: jwtOptions.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
