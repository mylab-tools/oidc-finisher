using System.Collections.ObjectModel;
using System.IdentityModel.Tokens.Jwt;

namespace MyLab.OidcFinisher.ApiSpecs.BizLogicApi;

public class TokenClaims : Collection<TokenClaim>
{
    public TokenClaims()
    {
            
    }

    public TokenClaims(IList<TokenClaim> initial)
        : base(initial)
    {
            
    }

    public static TokenClaims FromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(token);
        var preparedClaims = jwtSecurityToken.Claims
            .Select(c => new TokenClaim(c.Type, c.Value))
            .ToList();

        return new TokenClaims(preparedClaims);
    }
}