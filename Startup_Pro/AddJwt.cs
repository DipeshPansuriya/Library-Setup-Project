using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InfraLib.Startup_Pro
{
    public static class AddJwt
    {
        public static void Builder(WebApplicationBuilder builder, string[] keys)
        {
            AuthenticationBuilder authenticationBuilder = builder.Services
                .AddAuthentication(
                    x =>
                    {
                        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    });
            for (int i = 0; i < keys.Length; i++)
            {
                byte[] key = Encoding.ASCII.GetBytes(keys[i]);
                _ = authenticationBuilder.AddJwtBearer(
                    $"Token{i + 1}",
                    x =>
                    {
                        x.RequireHttpsMetadata = false;
                        x.SaveToken = true;
                        x.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(key),
                            ValidateIssuer = false,
                            ValidateAudience = false
                        };
                    });
            }

            _ = builder.Services
                .AddAuthorization(
                    options =>
                    {
                        AuthorizationPolicyBuilder policyBuilder = new AuthorizationPolicyBuilder().RequireAuthenticatedUser(
                            );
                        for (int i = 0; i < keys.Length; i++)
                        {
                            policyBuilder = policyBuilder.AddAuthenticationSchemes($"Token{i + 1}");
                        }
                        options.DefaultPolicy = policyBuilder.Build();
                    });
        }


        public static void App(WebApplication app)
        {
            _ = app.UseAuthentication();
            _ = app.UseAuthorization();
        }


        public static TokenDetails GenerateToken(
            string username,
            string secretKey,
            int expireMinutes = 20,
            string role = "default")
        {
            byte[] keyBytes = Encoding.ASCII.GetBytes(secretKey);
            SymmetricSecurityKey symmetricKey = new(keyBytes);
            JwtSecurityTokenHandler tokenHandler = new();
            DateTime now = DateTime.UtcNow;

            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject =
                    new ClaimsIdentity(
                        new[] { new Claim(ClaimTypes.Name, username), new Claim(ClaimTypes.Role, role) }),
                Expires = now.AddMinutes(expireMinutes),
                SigningCredentials =
                    new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256Signature)
            };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            TokenDetails tokenDetails = new()
            {
                issued_at_UTC = token.ValidFrom,
                issued_at = Convert.ToString(((DateTimeOffset)token.ValidFrom).ToUnixTimeMilliseconds()),
                tokenType = "Bearer",
                status = "Generated",
                expires_in_UTC = token.ValidTo,
                expires_in = Convert.ToInt32((token.ValidTo - token.ValidFrom).TotalSeconds),
                access_token = tokenHandler.WriteToken(token)
            };

            return tokenDetails;
        }
    }
}