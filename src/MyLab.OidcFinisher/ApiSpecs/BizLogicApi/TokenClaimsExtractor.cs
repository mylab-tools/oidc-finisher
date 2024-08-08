using System.Collections.ObjectModel;
using System.IdentityModel.Tokens.Jwt;

namespace MyLab.OidcFinisher.ApiSpecs.BizLogicApi;

public static class TokenClaimsExtractor 
{
    public static IReadOnlyDictionary<string, string> FromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(token);
        var preparedClaims = jwtSecurityToken.Claims
            .GroupBy(c => c.Type, c => c.Value)
            .Select(g => new { Key = g.Key, Value = g.First() })
            .ToDictionary(g => g.Key, g => g.Value);

        return preparedClaims.AsReadOnly();
    }
}