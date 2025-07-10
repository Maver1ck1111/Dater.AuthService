using Dater.Auth.Application;
using Dater.Auth.Application.DTOs;
using Dater.Auth.Application.RepositoriesContracts;
using Dater.Auth.Application.Services;
using Dater.Auth.Application.ServicesContracts;
using Dater.Auth.Domain;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Dater.Auth.Test
{
    public class AuthServiceTest
    {
        private readonly Mock<IHasherService> _hasherServiceMock;
        private readonly Mock<IJWTService> _jwtServiceMock;
        private readonly Mock<IAccountRepository> _accountRepositoryMock;
        private readonly Mock<ILogger<AuthService>> _loggerMock;

        public AuthServiceTest()
        {
            _hasherServiceMock = new Mock<IHasherService>();
            _jwtServiceMock = new Mock<IJWTService>();
            _accountRepositoryMock = new Mock<IAccountRepository>();
            _loggerMock = new Mock<ILogger<AuthService>>();
        }

        [Fact]
        public async Task Login_ShouldReturnCorrectResponse()
        {
            AccountRequestDTO requestUser = new AccountRequestDTO
            {
                Email = "someEmail@mail.com",
                Password = "password12345"
            };

            Account account = new Account
            {
                Email = requestUser.Email,
                HashedPassword = "hashedPassword1234"
            };

            _accountRepositoryMock
                .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(Result<Account>.OnSuccess(account));

            _accountRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<Account>()))
                .ReturnsAsync(Result<bool>.OnSuccess(true));

            _hasherServiceMock
                .Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            _jwtServiceMock
                .Setup(x => x.GenerateRefreshToken())
                .Returns("2134516216");

            _jwtServiceMock
                .Setup(x => x.GenerateToken(It.IsAny<string>()))
                .Returns("myJwtToken");

            var AuthService = new AuthService(
                _hasherServiceMock.Object,
                _jwtServiceMock.Object,
                _accountRepositoryMock.Object,
                _loggerMock.Object
            );

            Result<AccountResponseDTO> result = await AuthService.Login(requestUser);

            result.StatusCode.Should().Be(200);

            result.Value?.Email.Should().Be(requestUser.Email);
            
            result.Value?.AccessToken.Should().NotBeNullOrEmpty();

            result.Value?.RefreshToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_ShouldReturnNotFoundForIncorrectEmail()
        {
            AccountRequestDTO requestDTO = new AccountRequestDTO
            {
                Email = "wrongEmail@mail.com",
                Password = "password12345"
            };

            _accountRepositoryMock
               .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
               .ReturnsAsync(Result<Account>.OnError(404, "User not found"));

            var AuthService = new AuthService(
               _hasherServiceMock.Object,
               _jwtServiceMock.Object,
               _accountRepositoryMock.Object,
               _loggerMock.Object
            );

            Result<AccountResponseDTO> result = await AuthService.Login(requestDTO);

            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorizedForIncorrectPassword()
        {
            AccountRequestDTO requestDTO = new AccountRequestDTO
            {
                Email = "Email@mail.com",
                Password = "wrongPassword12345"
            };

            _accountRepositoryMock
               .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
               .ReturnsAsync(Result<Account>.OnSuccess(new Account
               {
                   Email = requestDTO.Email,
                   HashedPassword = "hashedPassword1234"
               }, 200));

            _hasherServiceMock
                .Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);

            var AuthService = new AuthService(
               _hasherServiceMock.Object,
               _jwtServiceMock.Object,
               _accountRepositoryMock.Object,
               _loggerMock.Object
            );

            Result<AccountResponseDTO> result = await AuthService.Login(requestDTO);

            result.StatusCode.Should().Be(401);
        }


        [Fact]
        public async Task Register_ShouldReturnCorrectResponse()
        {
            AccountRequestDTO accountRequest = new AccountRequestDTO
            {
                Email = "someEmail@mail.com",
                Password = "password12345"
            };

            _accountRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Account>()))
                .ReturnsAsync(Result<Guid>.OnSuccess(Guid.NewGuid(), 201));

            _jwtServiceMock
                .Setup(x => x.GenerateRefreshToken())
                .Returns("someRefreshToken1235");

            _jwtServiceMock
                .Setup(x => x.GenerateToken(It.IsAny<string>()))
                .Returns("someJwtToken");

            _hasherServiceMock
                .Setup(x => x.HashPassword(It.IsAny<string>()))
                .Returns("hashedPassword1234");

            var AuthService = new AuthService(
               _hasherServiceMock.Object,
               _jwtServiceMock.Object,
               _accountRepositoryMock.Object,
               _loggerMock.Object
            );

            Result<AccountResponseDTO> result = await AuthService.Register(accountRequest);

            result.StatusCode.Should().Be(201);
            result.Value.Should().NotBeNull();

            result.Value.AccessToken.Should().NotBeNullOrEmpty();
            result.Value.RefreshToken.Should().NotBeNullOrEmpty();
            result.Value.Email.Should().Be(accountRequest.Email);
        }

        [Fact]
        public async Task RefreshToken_ShouldReturnCorrectResponse()
        {
            string email = "someEmail@mail.com";

            _accountRepositoryMock
                .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(Result<Account>.OnSuccess(new Account
                {
                    Email = email,
                    RefreshToken = "validRefreshToken1234",
                    RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(3)
                }));

            _jwtServiceMock
                .Setup(x => x.GenerateToken(It.IsAny<string>()))
                .Returns("newJwtToken");

            _jwtServiceMock
                .Setup(x => x.GenerateRefreshToken())
                .Returns("newRefreshToken1234");

            var AuthService = new AuthService(
               _hasherServiceMock.Object,
               _jwtServiceMock.Object,
               _accountRepositoryMock.Object,
               _loggerMock.Object
            );

            Result<AccountResponseDTO> result = await AuthService.RefreshTokens(email, "validRefreshToken1234");

            result.StatusCode.Should().Be(200);

            result.Value.Should().NotBeNull();
            result.Value.Email.Should().Be(email);

            result.Value.AccessToken.Should().NotBeNullOrEmpty();
            result.Value.RefreshToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task RefreshToken_ShouldReturn401Response_IncorrectToken()
        {
            string email = "someEmail@mail.com";

            _accountRepositoryMock
                .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(Result<Account>.OnSuccess(new Account
                {
                    Email = email,
                    RefreshToken = "validRefreshToken1234"
                }));

            var AuthService = new AuthService(
               _hasherServiceMock.Object,
               _jwtServiceMock.Object,
               _accountRepositoryMock.Object,
               _loggerMock.Object
            );

            Result<AccountResponseDTO> result = await AuthService.RefreshTokens(email, "anotherRefreshToken");

            result.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task RefreshToken_ShouldReturn401Response_TimeExpire() 
        {
            string email = "someEmail@mail.com";

            _accountRepositoryMock
                .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(Result<Account>.OnSuccess(new Account
                {
                    Email = email,
                    RefreshToken = "validRefreshToken1234",
                    RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(-10)
                }));

            var AuthService = new AuthService(
               _hasherServiceMock.Object,
               _jwtServiceMock.Object,
               _accountRepositoryMock.Object,
               _loggerMock.Object
            );

            Result<AccountResponseDTO> result = await AuthService.RefreshTokens(email, "validRefreshToken1234");

            result.StatusCode.Should().Be(401);
        }
    }
}
