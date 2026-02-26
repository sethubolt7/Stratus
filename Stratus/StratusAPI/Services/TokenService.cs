using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StratusAPI.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        public TokenService(IConfiguration config) => _config = config;

        // create signed JWT string
        public string GenerateAccessToken(string username)
        {
            var key = Encoding.ASCII.GetBytes(_config["JwtSettings:SecretKey"]);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iss, _config["JwtSettings:Issuer"]),
                new Claim(JwtRegisteredClaimNames.Aud, _config["JwtSettings:Audience"]),
                new Claim(JwtRegisteredClaimNames.Exp, 
                          new DateTimeOffset(DateTime.UtcNow.AddMinutes(
                             int.Parse(_config["JwtSettings:AccessTokenExpiryMinutes"])
                          )).ToUnixTimeSeconds().ToString())
            };

            var token = new JwtSecurityToken(
                claims: claims,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // create a random opaque refresh token
        public string GenerateRefreshToken() => Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }
}
