using FinalProjApp.Models;
using FinalProjApp.Services;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace FinalProjApp.Web.Api
{
    /// <summary>Issues the tokens the phone app sends with every API call.</summary>
    public class TokenService
    {
        public const string Issuer = "calybear-web";
        public const string Audience = "calybear-app";
        public static readonly TimeSpan Lifetime = TimeSpan.FromDays(30);

        private readonly SymmetricSecurityKey _key;

        public TokenService(IConfiguration config) => _key = SigningKey(config);

        /// <summary>From Jwt:Key (user secrets locally, an app setting on Azure). At least 32 characters.</summary>
        public static SymmetricSecurityKey SigningKey(IConfiguration config)
        {
            var key = config["Jwt:Key"];
            if (string.IsNullOrEmpty(key) || key.Length < 32)
                throw new InvalidOperationException(
                    "Set Jwt:Key (32+ characters): `dotnet user-secrets set Jwt:Key <value>` locally, or the Jwt__Key app setting on Azure.");
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        }

        public LoginResponse Issue(AppUser user)
        {
            var expires = DateTime.UtcNow.Add(Lifetime);
            var principal = AuthClaims.CreatePrincipal(user, "Bearer");
            var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
            {
                Subject = (System.Security.Claims.ClaimsIdentity)principal.Identity!,
                Issuer = Issuer,
                Audience = Audience,
                Expires = expires,
                SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256)
            });

            return new LoginResponse
            {
                Token = token,
                ExpiresUtc = expires,
                UserId = user.UserId,
                Email = user.Email,
                Role = user.Role,
                GuardianId = user.GuardianId
            };
        }
    }
}
