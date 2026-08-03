using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerce.Application.Abstractions.Authentication;
using ECommerce.Domain.Entities.User;
using ECommerce.Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.Infrastructure.Library.Authentication;

public class JwtGenerator(IUserClaimsFactory claimsFactory, IOptions<JwtOptions> jwtOptions) : IJwtGenerator
{
    public async Task<string> GenerateTokenAsync(User user, CancellationToken cancellationToken)
    { 
        var claims = CreateClaims(user);
        var roleClaims = await claimsFactory.CreateRoleClaimsAsync(user, cancellationToken);
        var allClaims = claims.Concat(roleClaims);
          
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.Secret))
        {
            KeyId = jwtOptions.Value.KeyId
        };
        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256); 
 
        var token = new JwtSecurityToken(
            issuer: jwtOptions.Value.Issuer,
            audience: jwtOptions.Value.Audience,
            claims: allClaims,
            expires: DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static Claim[] CreateClaims(User user)
    {
        Claim[] claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
        ];

        return claims;
    }
}
