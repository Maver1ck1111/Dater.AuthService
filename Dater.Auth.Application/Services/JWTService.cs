using Dater.Auth.Application.ServicesContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Dater.Auth.Application.Services
{
    public class JWTService : IJWTService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JWTService> _logger;
        public JWTService(IConfiguration configuration, ILogger<JWTService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            _logger.LogInformation("Was successfully created a refresh token");

            return Convert.ToBase64String(randomBytes);
        }

        public string GenerateToken(string email)
        {
            Claim[] claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Email, email)
            };

            SymmetricSecurityKey symetricalKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));

            SigningCredentials signingCredentials = new SigningCredentials(symetricalKey, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JWT:ExpireMinutes"])),
                signingCredentials: signingCredentials
            );

            _logger.LogInformation("Was successfully created a access token for {email} expires at {expires}", email, jwtSecurityToken.ValidTo);

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }
    }
}
