using System.Security.Claims;

namespace VNext.JWT;

public interface ITokenService
{
  string BuildToken(IEnumerable<Claim> claims, JWTOptions options);
}