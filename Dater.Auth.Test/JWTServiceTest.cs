using Castle.Core.Logging;
using Dater.Auth.Application.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Dater.Auth.Test
{
    public class JWTServiceTest
    {
        private readonly IConfiguration _configuration;
        private readonly Mock<ILogger<JWTService>> _mockLogger = new Mock<ILogger<JWTService>>();

        public JWTServiceTest()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                { "JWT:Key", "ThisIsASecureKey123456789012345678901234567890" },
                { "JWT:Issuer", "TestIssuer" },
                { "JWT:Audience", "TestAudience" },
                { "JWT:ExpireMinutes", "10" }
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _configuration = configuration;
        }

        [Fact]
        public void GenerateToken_ShouldReturnToken()
        {
            string token = new JWTService(_configuration, _mockLogger.Object).GenerateToken("SomeEmail@mail.com");
            token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void GenerateToken_ShouldReturnTokenWithCorrectClaims()
        {
            string token = new JWTService(_configuration, _mockLogger.Object).GenerateToken("SomeEmail@mail.com");

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

            IEnumerable<Claim> claims = jwtToken.Claims;

            Claim? email = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);

            email.Should().NotBeNull();
            email.Value.Should().Be("SomeEmail@mail.com");

            Claim? jti = claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti);

            jti.Should().NotBeNull();
            jti.Value.Should().NotBeNullOrEmpty();

            jwtToken.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(10), TimeSpan.FromSeconds(5));

            jwtToken.Audiences.FirstOrDefault().Should().Be(_configuration["JWT:Audience"]);

            jwtToken.Issuer.Should().Be(_configuration["JWT:Issuer"]);
        }

        [Fact]
        public void GenerateRefreshToken_ShouldReturnNonEmptyString()
        {
            string refreshToken = new JWTService(_configuration, _mockLogger.Object).GenerateRefreshToken();
            refreshToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void GenerateRefreshToken_ShouldReturnUniqueValue()
        {
            var service = new JWTService(_configuration, _mockLogger.Object);
            string refreshToken1 = service.GenerateRefreshToken();
            refreshToken1.Should().NotBeNullOrEmpty();

            string refreshToken2 = service.GenerateRefreshToken();
            refreshToken1.Should().NotBeNullOrEmpty();

            refreshToken1.Should().NotBe(refreshToken2);
        }
    }
}
